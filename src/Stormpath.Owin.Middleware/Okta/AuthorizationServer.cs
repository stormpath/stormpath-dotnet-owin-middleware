using System.Collections.Generic;

namespace Stormpath.Owin.Middleware.Okta
{
    public sealed class AuthorizationServer
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Audiences { get; set; }

        public string Issuer { get; set; }
    }
}
