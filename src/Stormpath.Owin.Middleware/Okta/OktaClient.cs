using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Stormpath.Owin.Middleware.Okta
{
    public sealed class OktaClient : IOktaClient
    {
        private const string ApiPrefix = "api/v1";
        private const string DefaultPasswordGrantScopes = "openid offline_access";

        private readonly string _orgUrl;
        private readonly string _apiToken;
        private readonly ILogger _logger;

        private readonly HttpClient _httpClient;

        public OktaClient(string orgUrl, string apiToken, ILogger logger)
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

            _httpClient = CreateClient(_orgUrl);
            _logger.LogTrace($"Client configured to connect to {_orgUrl}");
        }

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

        private static HttpClient CreateClient(string orgBaseUrl)
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

            return client;
        }

        private async Task<T> GetResource<T>(string path, CancellationToken cancellationToken)
        {
            // orgUrl already is guaranteed to have a trailing slash
            var sanitizedResourcePath = path.TrimStart('/');

            using (var request = new HttpRequestMessage(HttpMethod.Get, sanitizedResourcePath))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("SSWS", _apiToken);
                return await SendAsync<T>(request, cancellationToken);
            }
        }

        private const string DefaultErrorMessage = "HTTP request failure";

        private Exception DefaultExceptionFormatter(int statusCode, string body)
        {
            _logger.LogWarning($"{statusCode} {body}");
            return new InvalidOperationException(DefaultErrorMessage);
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

        private Task SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            Func<int, string, Exception> exceptionFormatter = null)
            => SendAsync<IDictionary<string, object>>(request, cancellationToken, exceptionFormatter);

        private async Task<T> SendAsync<T>(
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            Func<int, string, Exception> exceptionFormatter = null)
        {
            exceptionFormatter = exceptionFormatter ?? DefaultExceptionFormatter;

            _logger.LogTrace($"{request.Method} {request.RequestUri}");

            using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogTrace($"{response.StatusCode} {request.RequestUri.PathAndQuery}");

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return response.IsSuccessStatusCode
                    ? JsonConvert.DeserializeObject<T>(json)
                    : throw exceptionFormatter((int)response.StatusCode, json);
            }
        }

        public Task<Application> GetApplicationAsync(string appId, CancellationToken cancellationToken)
            => GetResource<Application>($"{ApiPrefix}/apps/{appId}", cancellationToken);

        public Task<ApplicationClientCredentials> GetClientCredentialsAsync(string appId, CancellationToken cancellationToken)
            => GetResource<ApplicationClientCredentials>($"{ApiPrefix}/internal/apps/{appId}/settings/clientcreds", cancellationToken);

        public Task<User> GetUserAsync(string userId, CancellationToken cancellationToken)
            => GetResource<User>($"{ApiPrefix}/users/{userId}", cancellationToken);

        public async Task<User> CreateUserAsync(
            dynamic profile,
            string password,
            string recoveryQuestion,
            string recoveryAnswer,
            CancellationToken cancellationToken)
        {
            var url = $"{ApiPrefix}/users?activate=true";

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("SSWS", _apiToken);

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
                
                return await SendAsync<User>(request, cancellationToken);
            }
        }

        public async Task AddUserToAppAsync(string appId, string userId, string email, CancellationToken cancellationToken)
        {
            var url = $"{ApiPrefix}/apps/{appId}/users";

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("SSWS", _apiToken);

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

                await SendAsync(request, cancellationToken);
                return;
            }
        }

        public async Task<GrantResult> PostPasswordGrantAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string username,
            string password,
            CancellationToken cancellationToken)
        {
            var url = $"oauth2/{authorizationServerId}/v1/token";

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var parameters = new Dictionary<string, string>()
                {
                    ["grant_type"] = "password",
                    ["scope"] = DefaultPasswordGrantScopes,
                    ["username"] = username,
                    ["password"] = password
                };
                request.Content = new FormUrlEncodedContent(parameters);

                _logger.LogTrace($"Executing password grant flow for subject {username}");

                var exceptionFormatter = new Func<int, string, Exception>((_, json) =>
                {
                    // TODO this always says "Invalid grant" for bad username/password
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (!data.TryGetValue("error_description", out string message))
                    {
                        message = "Invalid request";
                    }

                    return new Exception(message);
                });

                return await SendAsync<GrantResult>(request, cancellationToken, exceptionFormatter);
            }
        }

        public async Task<GrantResult> PostRefreshGrantAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string refreshToken,
            CancellationToken cancellationToken)
        {
            var url = $"oauth2/{authorizationServerId}/v1/token";

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var parameters = new Dictionary<string, string>()
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refreshToken
                };
                request.Content = new FormUrlEncodedContent(parameters);

                _logger.LogTrace($"Executing refresh grant flow with token {refreshToken}");

                var exceptionFormatter = new Func<int, string, Exception>((_, json) =>
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (!data.TryGetValue("error_description", out string message))
                    {
                        message = "Invalid request";
                    }

                    return new Exception(message);
                });

                return await SendAsync<GrantResult>(request, cancellationToken, exceptionFormatter);
            }
        }

        public async Task<TokenIntrospectionResult> IntrospectTokenAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string token,
            string tokenType, 
            CancellationToken cancellationToken)
        {
            var url = $"oauth2/{authorizationServerId}/v1/introspect";

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var parameters = new Dictionary<string, string>()
                {
                    ["token"] = token,
                    ["token_type_hint"] = tokenType
                };
                request.Content = new FormUrlEncodedContent(parameters);

                // todo why can't I remove this await?
                return await SendAsync<TokenIntrospectionResult>(request, cancellationToken);
            }
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
            request.Headers.Authorization = new AuthenticationHeaderValue("SSWS", _apiToken);

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
            request.Headers.Authorization = new AuthenticationHeaderValue("SSWS", _apiToken);

            var body = new { stateToken, answer };
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            return SendAsync<RecoveryTransactionObject>(request, cancellationToken, SummaryFormatter);
        }

        public Task<RecoveryTransactionObject> ResetPasswordAsync(string stateToken, string newPassword, CancellationToken cancellationToken)
        {
            var url = $"{ApiPrefix}/authn/credentials/reset_password";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("SSWS", _apiToken);

            var body = new { stateToken, newPassword };
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            return SendAsync<RecoveryTransactionObject>(request, cancellationToken, SummaryFormatter);
        }
    }
}
