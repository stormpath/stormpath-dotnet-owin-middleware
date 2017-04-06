using Stormpath.Owin.Middleware.Okta;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware
{
    public sealed class RemoteTokenValidator
    {
        private readonly IOktaClient _oktaClient;
        private readonly string _authorizationServerId;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public RemoteTokenValidator(
            IOktaClient oktaClient,
            string authorizationServerId,
            string clientId,
            string clientSecret)
        {
            _oktaClient = oktaClient;
            _authorizationServerId = authorizationServerId;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public Task<TokenIntrospectionResult> ValidateAsync(
            string token,
            string tokenType,
            CancellationToken cancellationToken)
        {
            return _oktaClient.IntrospectTokenAsync(
                _authorizationServerId,
                _clientId,
                _clientSecret,
                token,
                tokenType,
                cancellationToken);
        }
    }
}
