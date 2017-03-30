using Newtonsoft.Json;

namespace Stormpath.Owin.Middleware
{
    public class GrantResult
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        public string Scope { get; set; }
    }
}
