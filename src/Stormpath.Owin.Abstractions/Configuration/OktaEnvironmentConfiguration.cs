namespace Stormpath.Owin.Abstractions.Configuration
{
    public sealed class OktaEnvironmentConfiguration
    {
        public OktaEnvironmentConfiguration(
            string authorizationServerId,
            string clientId,
            string clientSecret)
        {
            AuthorizationServerId = authorizationServerId;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public string AuthorizationServerId { get; }

        public string ClientId { get; }

        public string ClientSecret { get; }
    }
}
