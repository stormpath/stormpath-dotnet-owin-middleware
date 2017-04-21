using Newtonsoft.Json;

namespace Stormpath.Owin.Middleware.Okta
{
    public sealed class ApplicationClientCredentials
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("token_endpoint_auth_method")]
        public string TokenEndpointAuthMethod { get; set; }
    }
}
