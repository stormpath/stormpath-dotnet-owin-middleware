using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Stormpath.Owin.Middleware.Okta
{
    public sealed class OktaClient
    {
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

            _httpClient = CreateClient();
            _logger.LogTrace($"Client configured to connect to {_orgUrl}");
        }

        private static string EnsureCorrectOrgUrl(string orgUrl)
        {
            if (!orgUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Org URL must start with https://");
            }

            if (!orgUrl.EndsWith("/api/v1/"))
            {
                orgUrl.TrimEnd('/');
                orgUrl += "/api/v1/";
            }

            return orgUrl;
        }

        private HttpClient CreateClient()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            var client = new HttpClient(handler, true)
            {
                BaseAddress = new Uri(_orgUrl, UriKind.Absolute)
            };

            // Workaround for https://github.com/dotnet/corefx/issues/11224
            client.DefaultRequestHeaders.Add("Connection", "close");

            return client;
        }

        public async Task<T> GetResource<T>(string path)
        {
            // orgUrl already is guaranteed to have a trailing slash
            var sanitizedResourcePath = path.TrimStart('/');

            using (var request = new HttpRequestMessage(HttpMethod.Get, sanitizedResourcePath))
            {
                // TODO move this to DefaultRequestHeader?
                request.Headers.Authorization = new AuthenticationHeaderValue("SSWS", _apiToken);

                _logger.LogTrace($"Getting resource from {path}");

                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    _logger.LogTrace($"{response.StatusCode} {path}");
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
        }

        public Task<ApplicationClientCredentials> GetClientCredentials(string appId)
        {
            return GetResource<ApplicationClientCredentials>($"internal/apps/{appId}/settings/clientcreds");
        }
    }
}
