using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stormpath.Owin.Middleware.Internal;

namespace Stormpath.Owin.Middleware.Okta
{
    public sealed class OktaClient : IOktaClient
    {
        private const string OktaClientUserAgent = "stormpath-oktagration";
        private const string ApiPrefix = "api/v1";
        private const string DefaultPasswordGrantScopes = "openid offline_access";

        private readonly string _orgUrl;
        private readonly string _apiToken;
        private readonly ILogger _logger;

        private readonly HttpClient _httpClient;
        private readonly string _userAgent;

        private readonly IDistributedCache _cacheProvider;
        private readonly IDictionary<Type, DistributedCacheEntryOptions> _cacheEntryOptions;

        public OktaClient(
            string orgUrl,
            string apiToken,
            IFrameworkUserAgentBuilder userAgentBuilder,
            IDistributedCache cacheProvider,
            IDictionary<Type, DistributedCacheEntryOptions> cacheEntryOptions,
            ILogger logger)
        {
            if (string.IsNullOrEmpty(orgUrl))
            {
                throw new ArgumentNullException(nameof(orgUrl));
            }

            if (string.IsNullOrEmpty(apiToken))
            {
                throw new ArgumentNullException(nameof(apiToken));
            }

            _apiToken = apiToken;
            _logger = logger;
            _orgUrl = EnsureCorrectOrgUrl(orgUrl);

            _userAgent = CreateUserAgent(userAgentBuilder);

            _httpClient = CreateClient(_orgUrl, _userAgent);
            _logger.LogTrace($"Client configured to connect to {_orgUrl}");

            _cacheProvider = cacheProvider;
            _cacheEntryOptions = cacheEntryOptions;
        }

        private static string CreateUserAgent(IFrameworkUserAgentBuilder userAgentBuilder)
            => $"{OktaClientUserAgent} {userAgentBuilder.GetUserAgent()}";

        private static string EnsureCorrectOrgUrl(string orgUrl)
        {
            if (!orgUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Org URL must start with https://");
            }

            if (!orgUrl.EndsWith("/"))
            {
                orgUrl += "/";
            }

            return orgUrl;
        }

        private static HttpClient CreateClient(string orgBaseUrl, string userAgent)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            var client = new HttpClient(handler, true)
            {
                BaseAddress = new Uri(orgBaseUrl, UriKind.Absolute)
            };

            // Workaround for https://github.com/dotnet/corefx/issues/11224
            client.DefaultRequestHeaders.Add("Connection", "close");

            client.DefaultRequestHeaders.Add("User-Agent", userAgent);

            return client;
        }

        private string BuildCacheKey(string type, string path)
        {
            // Cache keys are the object types concatenated with the full, absolute URL:
            // User:https://dev-123.oktapreview.com/api/v1/users/x1y2z3

            return $"{type}:{_orgUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        }

        private DistributedCacheEntryOptions GetCacheOptions<T>()
        {
            if (_cacheEntryOptions != null && _cacheEntryOptions.TryGetValue(typeof(T), out var options))
            {
                return options;
            }

            // Default cache entry options
            return new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
        }

        private const string DefaultErrorMessage = "HTTP request failure";

        private Exception DefaultExceptionFormatter(int statusCode, string body)
        {
            _logger.LogWarning($"{statusCode} {body}");

            IDictionary<string, object> bodyDictionary = null;

            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    bodyDictionary = JsonConvert.DeserializeObject<IDictionary<string, object>>(body);
                }
                catch (Exception)
                {
                }
            }

            var ex = new OktaException(DefaultErrorMessage);

            if (bodyDictionary != null)
            {
                ex.Body = bodyDictionary;
            }

            return ex;
        }

        private Exception SummaryFormatter(int statusCode, string body)
        {
            _logger.LogWarning($"{statusCode} {body}");

            try
            {
                var deserialized = JsonConvert.DeserializeObject<ApiError>(body);
                if (string.IsNullOrEmpty(deserialized?.ErrorSummary)) return DefaultExceptionFormatter(statusCode, body);
                return new InvalidOperationException(deserialized.ErrorSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(1005, ex, "Error while formatting error response");
                return DefaultExceptionFormatter(statusCode, body);
            }
        }

        private static void AddClientCredentialsAuth(HttpRequestMessage request, string clientId, string clientSecret)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        private void AddSswsAuth(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("SSWS", _apiToken);
        }

        private async Task<T> GetCachedResource<T>(string path, CancellationToken cancellationToken)
        {
            var resourceJson = string.Empty;
            var typeName = typeof(T).Name;

            // If there's a cache, try to retrieve from the cache
            if (_cacheProvider != null)
            {
                resourceJson = await _cacheProvider.GetStringAsync(BuildCacheKey(typeName, path));
            }

            if (!string.IsNullOrEmpty(resourceJson))
            {
                return Deserialize<T>(resourceJson);
            }

            resourceJson = await GetResource(path, cancellationToken);

            // If there's a cache, save this resource
            if (_cacheProvider != null)
            {
                await _cacheProvider.SetStringAsync(BuildCacheKey(typeName, path), resourceJson, GetCacheOptions<T>());
            }

            return Deserialize<T>(resourceJson);
        }

        private Task<string> GetResource(string path, CancellationToken cancellationToken)
        {
            // orgUrl already is guaranteed to have a trailing slash
            var sanitizedResourcePath = path.TrimStart('/');

            var request = new HttpRequestMessage(HttpMethod.Get, sanitizedResourcePath);
            AddSswsAuth(request);
            return SendAsync(request, cancellationToken);
        }

        private async Task<T> GetResource<T>(string path, CancellationToken cancellationToken)
        {
            var json = await GetResource(path, cancellationToken);
            return Deserialize<T>(json);
        }

        private T Deserialize<T>(string json)
            => JsonConvert.DeserializeObject<T>(json);

        private async Task<T> SendAsync<T>(
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            Func<int, string, Exception> exceptionFormatter = null)
        {
            var json = await SendAsync(request, cancellationToken, exceptionFormatter);
            return Deserialize<T>(json);
        }

        private async Task<string> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            Func<int, string, Exception> exceptionFormatter = null)
        {
            exceptionFormatter = exceptionFormatter ?? DefaultExceptionFormatter;

            _logger.LogTrace($"{request.Method} {request.RequestUri}");

            using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogTrace($"{(int)response.StatusCode} {request.RequestUri.PathAndQuery}");

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return response.IsSuccessStatusCode
                    ? json
                    : throw exceptionFormatter((int)response.StatusCode, json);
            }
        }

        public Task<Application> GetApplicationAsync(string appId, CancellationToken cancellationToken)
            => GetResource<Application>($"{ApiPrefix}/apps/{appId}", cancellationToken);

        public Task<ApplicationClientCredentials> GetClientCredentialsAsync(string appId, CancellationToken cancellationToken)
            => GetResource<ApplicationClientCredentials>($"{ApiPrefix}/internal/apps/{appId}/settings/clientcreds", cancellationToken);

        public Task<User> GetUserAsync(string userId, CancellationToken cancellationToken)
            => GetCachedResource<User>($"{ApiPrefix}/users/{userId}", cancellationToken);

        public Task<List<User>> FindUsersByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var filter = $"profile.email eq \"{email}\"";
            return GetResource<List<User>>($"{ApiPrefix}/users?filter={filter}", cancellationToken);
        }

        public Task<List<User>> SearchUsersAsync(string searchExpression, CancellationToken cancellationToken)
            => GetResource<List<User>>($"{ApiPrefix}/users?search={searchExpression}", cancellationToken);

        public async Task<User> UpdateUserProfileAsync(string userId, IDictionary<string, object> updatedProfileProperties, CancellationToken cancellationToken)
        {
            var url = $"{ApiPrefix}/users/{userId}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddSswsAuth(request);

            var payload = new
            {
                profile = updatedProfileProperties
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            // If there's a cache, expire this user's entry
            if (_cacheProvider != null)
            {
                await _cacheProvider.RemoveAsync(BuildCacheKey(typeof(User).Name, url)); 
            }

            return await SendAsync<User>(request, cancellationToken);
        }

        public async Task ActivateUserAsync(string userId, CancellationToken cancellationToken)
        {
            var url = $"{ApiPrefix}/users/{userId}/lifecycle/activate?sendEmail=false";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddSswsAuth(request);

            // If there's a cache, expire this user's entry
            if (_cacheProvider != null)
            {
                await _cacheProvider.RemoveAsync(BuildCacheKey(typeof(User).Name, url)); 
            }

            await SendAsync<User>(request, cancellationToken);
            return;
        }

        public Task<User> CreateUserAsync(
            IDictionary<string, object> profile,
            string password,
            bool activate,
            string recoveryQuestion,
            string recoveryAnswer,
            CancellationToken cancellationToken)
        {
            var url = $"{ApiPrefix}/users?activate={activate.ToString().ToLower()}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddSswsAuth(request);

            var payload = new
            {
                profile,
                credentials = new
                {
                    password = new { value = password },
                    recovery_question = new { question = recoveryQuestion, answer = recoveryAnswer }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            return SendAsync<User>(request, cancellationToken);
        }

        public Task AddUserToAppAsync(string appId, string userId, string email, CancellationToken cancellationToken)
        {
            var url = $"{ApiPrefix}/apps/{appId}/users";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddSswsAuth(request);

            var payload = new
            {
                id = userId,
                scope = "USER",
                credentials = new
                {
                    userName = email
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            return SendAsync(request, cancellationToken);
        }

        public Task<GrantResult> PostPasswordGrantAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string username,
            string password,
            CancellationToken cancellationToken)
        {
            var url = $"oauth2/{authorizationServerId}/v1/token";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddClientCredentialsAuth(request, clientId, clientSecret);

            var parameters = new Dictionary<string, string>()
            {
                ["grant_type"] = "password",
                ["scope"] = DefaultPasswordGrantScopes,
                ["username"] = username,
                ["password"] = password
            };
            request.Content = new FormUrlEncodedContent(parameters);

            _logger.LogTrace($"Executing password grant flow for subject {username}");
            return SendAsync<GrantResult>(request, cancellationToken);
        }

        public Task<GrantResult> PostRefreshGrantAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string refreshToken,
            CancellationToken cancellationToken)
        {
            var url = $"oauth2/{authorizationServerId}/v1/token";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddClientCredentialsAuth(request, clientId, clientSecret);

            var parameters = new Dictionary<string, string>()
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            };
            request.Content = new FormUrlEncodedContent(parameters);

            _logger.LogTrace($"Executing refresh grant flow with token {refreshToken}");
            return SendAsync<GrantResult>(request, cancellationToken);
        }

        public Task<GrantResult> PostAuthCodeGrantAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string code,
            string originalRedirectUri,
            CancellationToken cancellationToken)
        {
            var url = $"oauth2/{authorizationServerId}/v1/token";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddClientCredentialsAuth(request, clientId, clientSecret);

            var parameters = new Dictionary<string, string>()
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = originalRedirectUri,
            };
            request.Content = new FormUrlEncodedContent(parameters);

            _logger.LogTrace($"Executing authorization code flow");
            return SendAsync<GrantResult>(request, cancellationToken);

        }

        public Task<TokenIntrospectionResult> IntrospectTokenAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string token,
            string tokenType,
            CancellationToken cancellationToken)
        {
            var url = $"oauth2/{authorizationServerId}/v1/introspect";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddClientCredentialsAuth(request, clientId, clientSecret);

            var parameters = new Dictionary<string, string>()
            {
                ["token"] = token,
                ["token_type_hint"] = tokenType
            };
            request.Content = new FormUrlEncodedContent(parameters);

            return SendAsync<TokenIntrospectionResult>(request, cancellationToken);
        }

        public Task RevokeTokenAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string token,
            string tokenType,
            CancellationToken cancellationToken)
        {
            var url = $"oauth2/{authorizationServerId}/v1/revoke";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddClientCredentialsAuth(request, clientId, clientSecret);

            var parameters = new Dictionary<string, string>()
            {
                ["token"] = token,
                ["token_type_hint"] = tokenType
            };
            request.Content = new FormUrlEncodedContent(parameters);

            return SendAsync(request, cancellationToken);
        }

        public Task SendPasswordResetEmailAsync(string login, CancellationToken cancellationToken)
        {
            var url = $"{ApiPrefix}/authn/recovery/password";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var body = new
            {
                username = login,
                factorType = "EMAIL"
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            return SendAsync(request, cancellationToken);
        }

        public Task<RecoveryTransactionObject> VerifyRecoveryTokenAsync(string token, CancellationToken cancellationToken)
        {
            var url = $"{ApiPrefix}/authn/recovery/token";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddSswsAuth(request);

            var body = new
            {
                recoveryToken = token
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            return SendAsync<RecoveryTransactionObject>(request, cancellationToken, SummaryFormatter);
        }

        public Task<RecoveryTransactionObject> AnswerRecoveryQuestionAsync(string stateToken, string answer, CancellationToken cancellationToken)
        {
            var url = $"{ApiPrefix}/authn/recovery/answer";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddSswsAuth(request);

            var body = new { stateToken, answer };
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            return SendAsync<RecoveryTransactionObject>(request, cancellationToken, SummaryFormatter);
        }

        public Task<RecoveryTransactionObject> ResetPasswordAsync(string stateToken, string newPassword, CancellationToken cancellationToken)
        {
            var url = $"{ApiPrefix}/authn/credentials/reset_password";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddSswsAuth(request);

            var body = new { stateToken, newPassword };
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            return SendAsync<RecoveryTransactionObject>(request, cancellationToken, SummaryFormatter);
        }

        public Task<IdentityProvider[]> GetIdentityProvidersAsync(CancellationToken ct)
            => GetResource<IdentityProvider[]>($"{ApiPrefix}/idps", ct);

        public Task<Group[]> GetGroupsForUserIdAsync(string userId, CancellationToken cancellationToken)
            => GetResource<Group[]>($"{ApiPrefix}/users/{userId}/groups", cancellationToken);

        private const string ProfileAttributeDoesNotExist = "E0000031";

        public async Task<ShimApiKey> GetApiKeyAsync(string apiKeyId, CancellationToken cancellationToken)
        {
            User foundUser = null;
            string foundKeypair = null;

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    var foundUsers = await SearchUsersAsync($"profile.stormpathApiKey_{i} sw \"{apiKeyId}\"", cancellationToken);

                    foundUser = foundUsers?.FirstOrDefault();

                    if (foundUser == null) continue;

                    foundUser.Profile.TryGetValue($"stormpathApiKey_{i}", out var rawValue);
                    foundKeypair = rawValue?.ToString();

                    var keypairTokens = foundKeypair?.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    var valid = keypairTokens?.Length == 2;

                    if (!valid) continue;

                    return new ShimApiKey
                    {
                        Id = keypairTokens[0],
                        Secret = keypairTokens[1],
                        Status = "ENABLED",
                        User = foundUser
                    };
                }
                catch (OktaException oex)
                {
                    object rawCode = null;
                    oex?.Body?.TryGetValue("errorCode", out rawCode);
                    var code = rawCode?.ToString();
                    if (string.IsNullOrEmpty(code)) throw;

                    // Code E0000031 means "the profile attribute doesn't exist"
                    if (code.Equals(ProfileAttributeDoesNotExist))
                    {
                        _logger.LogWarning($"The profile attribute 'profile.stormpathApiKey_{i}' should be added to your Universal Directory configuration.");
                        continue;
                    }
                }
            }

            return null;
        }

        public Task<AuthorizationServer> GetAuthorizationServerAsync(string authorizationServerId, CancellationToken cancellationToken)
            => GetResource<AuthorizationServer>($"{ApiPrefix}/authorizationServers/{authorizationServerId}", cancellationToken);
    }
}
