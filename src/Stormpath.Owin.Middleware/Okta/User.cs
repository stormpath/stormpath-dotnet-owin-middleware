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
        public IDictionary<string, object> Profile { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }
    }

    // todo
    //public class UserProfile
    //{
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public object MobilePhone { get; set; }
    //    public string Email { get; set; }
    //    public object SecondEmail { get; set; }
    //    public string Login { get; set; }
    //}

    public class Links
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
