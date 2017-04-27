using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Stormpath.Owin.Middleware.Okta
{
    public sealed class Application
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Label { get; set; }

        public string Status { get; set; }

        public DateTimeOffset? LastUpdated { get; set; }

        public DateTimeOffset? Created { get; set; }

        public string SignOnMode { get; set; }

        public ApplicationCredentials Credentials { get; set; }

        public ApplicationSettings Settings { get; set; }

        [JsonProperty("_links")]
        public ApplicationLinks Links { get; set; }
    }

    public sealed class SigningCredentials
    {
        [JsonProperty("kid")]
        public string KeyId { get; set; }
    }

    public sealed class ApplicationCredentials
    {
        public SigningCredentials Signing { get; set; }
    }

    public sealed class VpnSettings
    {
        public string Message { get; set; }
        public object HelpUrl { get; set; }
    }

    public sealed class NotificationSettings
    {
        public VpnSettings Vpn { get; set; }
    }

    public sealed class ApplicationSettings
    {
        public NotificationSettings Notifications { get; set; }
    }

    public sealed class ApplicationLinks
    {
        public List<Link> AppLinks { get; set; }
        public Link Users { get; set; }
        public Link Deactivate { get; set; }
        public Link Groups { get; set; }
    }
}
