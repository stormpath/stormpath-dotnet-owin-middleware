using System.Collections.Generic;
using System.Linq;

namespace Stormpath.Owin.Abstractions.Configuration
{
    public sealed class OktaEnvironmentConfiguration
    {
        public OktaEnvironmentConfiguration(
            string authorizationServerId,
            IEnumerable<string> validAudiences,
            string clientId,
            string clientSecret)
        {
            AuthorizationServerId = authorizationServerId;
            ValidAudiences = validAudiences.ToArray();
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public string AuthorizationServerId { get; }

        public string[] ValidAudiences { get; }

        public string ClientId { get; }

        public string ClientSecret { get; }
    }
}
