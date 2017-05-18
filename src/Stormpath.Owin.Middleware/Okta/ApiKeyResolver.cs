using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware.Okta
{
    public class ApiKeyResolver
    {
        private readonly IOktaClient _oktaClient;

        public ApiKeyResolver(IOktaClient oktaClient)
        {
            _oktaClient = oktaClient;
        }

        public async Task<ShimApiKey> LookupApiKeyIdAsync(string id, string secret, CancellationToken cancellationToken)
        {
            var apiKey = await _oktaClient.GetApiKeyAsync(id, cancellationToken);

            var validKey =
                apiKey != null &&
                apiKey.Status.Equals("enabled", StringComparison.OrdinalIgnoreCase) &&
                ConstantTimeComparer.Equals(apiKey.Secret, secret);

            if (!validKey) return null;

            var validAccount =
                apiKey.User != null &&
                apiKey.User.Status.Equals("active", StringComparison.OrdinalIgnoreCase);

            if (!validAccount) return null;

            return apiKey;
        }
    }
}
