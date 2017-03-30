using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        private async Task<T> GetResource<T>(string path)
        {
            // orgUrl already is guaranteed to have a trailing slash
            var sanitizedResourcePath = path.TrimStart('/');

            using (var request = new HttpRequestMessage(HttpMethod.Get, sanitizedResourcePath))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("SSWS", _apiToken);
                return await SendAsync<T>(request);
            }
        }

        private static Exception DefaultExceptionFormatter(string _) => new Exception("Invalid request");

        private async Task<T> SendAsync<T>(HttpRequestMessage request, Func<string, Exception> exceptionFormatter = null)
        {
            exceptionFormatter = exceptionFormatter ?? DefaultExceptionFormatter;

            _logger.LogTrace($"{request.Method} {request.RequestUri}");

            using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
            {
                _logger.LogTrace($"{response.StatusCode} {request.RequestUri.PathAndQuery}");

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return response.IsSuccessStatusCode
                    ? JsonConvert.DeserializeObject<T>(json)
                    : throw exceptionFormatter(json);
            }
        }

        public Task<Application> GetApplication(string appId)
            => GetResource<Application>($"{ApiPrefix}/apps/{appId}");

        public Task<ApplicationClientCredentials> GetClientCredentials(string appId)
            => GetResource<ApplicationClientCredentials>($"{ApiPrefix}/internal/apps/{appId}/settings/clientcreds");

        public async Task<GrantResult> PostPasswordGrant(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string username,
            string password)
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

                var exceptionFormatter = new Func<string, Exception>(json =>
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (!data.TryGetValue("error_description", out string message))
                    {
                        message = "Invalid request";
                    }

                    return new Exception(message);
                });

                return await SendAsync<GrantResult>(request, exceptionFormatter);
            }
        }
    }
}
