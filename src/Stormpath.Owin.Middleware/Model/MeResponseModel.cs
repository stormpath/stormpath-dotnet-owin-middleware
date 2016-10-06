using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stormpath.Owin.Middleware.Model
{
    public sealed class MeResponseModel
    {
        public string Href { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string GivenName { get; set; }

        public string MiddleName { get; set; }

        public string Surname { get; set; }

        public string FullName { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }

        public DateTimeOffset? PasswordModifiedAt { get; set; }

        public string EmailVerificationToken { get; set; }

        public string Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Applications { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object ApiKeys { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyDictionary<string, object> CustomData { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Directory { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object GroupMemberships { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Groups { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object ProviderData { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Tenant { get; set; } = null;
    }
}