using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Stormpath.Owin.Middleware.Okta
{
    public class User
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public DateTimeOffset? Created { get; set; }
        public DateTimeOffset? Activated { get; set; }
        public DateTimeOffset? StatusChanged { get; set; }
        public DateTimeOffset? LastLogin { get; set; }
        public DateTimeOffset? LastUpdated { get; set; }
        public DateTimeOffset? PasswordChanged { get; set; }
        public IDictionary<string, object> Profile { get; set; } = new Dictionary<string, object>();

        [JsonProperty("_links")]
        public UserLinks Links { get; set; }
    }

    public class UserLinks
    {
        public Link Suspend { get; set; }
        public Link ResetPassword { get; set; }
        public Link ExpirePassword { get; set; }
        public Link ForgotPassword { get; set; }
        public Link Self { get; set; }
        public Link ChangeRecoveryQuestion { get; set; }
        public Link Deactivate { get; set; }
        public Link ChangePassword { get; set; }
    }
}
