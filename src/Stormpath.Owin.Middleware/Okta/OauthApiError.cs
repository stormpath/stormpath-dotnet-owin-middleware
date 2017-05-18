using Newtonsoft.Json;

namespace Stormpath.Owin.Middleware.Okta
{
    public class OauthApiError
    {
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string Description { get; set; }
    }
}
