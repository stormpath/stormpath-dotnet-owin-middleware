using Newtonsoft.Json;

namespace Stormpath.Owin.Middleware.Okta
{
    public class IdentityProvider
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Created { get; set; }
        public string LastUpdated { get; set; }

        [JsonProperty("_links")]
        public IdpLinks Links { get; set; }
    }

    public class IdpLinks
    {
        public Link Authorize { get; set; }
        public Link ClientRedirectUri { get; set; }
    }
}
