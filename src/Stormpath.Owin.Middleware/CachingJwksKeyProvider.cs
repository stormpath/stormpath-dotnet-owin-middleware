using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware
{
    public sealed class CachingJwksKeyProvider : IKeyProvider
    {
        private readonly string _openIdConfigurationEndpoint;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        private object _accessLock = new object();
        private IEnumerable<SecurityKey> _keys = Enumerable.Empty<SecurityKey>();
        private DateTimeOffset _keyExpiration = DateTimeOffset.MinValue;

        public CachingJwksKeyProvider(string openIdConfigurationEndpoint, ILogger logger)
        {
            _openIdConfigurationEndpoint = openIdConfigurationEndpoint;
            _logger = logger;

            _httpClient = CreateClient();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RefreshKeysAsync(CancellationToken.None);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private static HttpClient CreateClient()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            var client = new HttpClient(handler, true);

            // Workaround for https://github.com/dotnet/corefx/issues/11224
            client.DefaultRequestHeaders.Add("Connection", "close");

            return client;
        }

        private async Task RefreshKeysAsync(CancellationToken cancellationToken)
        {
            var config = await OpenIdConnectConfigurationRetriever.GetAsync(_openIdConfigurationEndpoint, cancellationToken);

            _logger.LogTrace($"GET {config.JwksUri}");

            // Get the expiration time for the JWK set
            using (var response = await _httpClient.GetAsync(config.JwksUri, cancellationToken).ConfigureAwait(false))
            {
                if (!response.Content.Headers.TryGetValues("Expires", out var expiresValues)) return;
                if (!DateTimeOffset.TryParse(expiresValues.FirstOrDefault(), out var expiration)) return;

                lock (_accessLock)
                {
                    _keyExpiration = expiration;
                    _keys = config.SigningKeys;
                }
            }
        }

        public async Task<IssuerSigningKeyResolver> GetSigningKeyResolver(CancellationToken cancellationToken)
        {
            bool needsRefresh = !_keys.Any() || _keyExpiration <= DateTimeOffset.UtcNow;
            if (needsRefresh)
            {
                await RefreshKeysAsync(cancellationToken);
            }

            return new IssuerSigningKeyResolver((token, securityToken, keyIdentifier, tokenValidationParameters) =>
            {
                return _keys;
            });
        }
    }
}
