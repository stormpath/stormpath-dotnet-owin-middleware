namespace Stormpath.Owin.Abstractions.Configuration
{
    public sealed class OktaEnvironmentConfiguration
    {
        public OktaEnvironmentConfiguration(
            string clientId,
            string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}
