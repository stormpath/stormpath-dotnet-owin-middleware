using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stormpath.Owin.Middleware.Okta
{
    public class RecoveryTransactionUser
    {
        public string Id { get; set; }
        public DateTimeOffset? PasswordChanged { get; set; }
        public IDictionary<string, object> Profile { get; set; }
    }

    public class RecoveryTransactionEmbedded
    {
        public RecoveryTransactionUser User { get; set; }
        public RecoveryTransactionPolicy Policy { get; set; }
    }

    public class RecoveryTransactionPolicy
    {
        public PasswordExpirationPolicy Expiration { get; set; }
        public PasswordComplexityPolicy Complexity { get; set; }
        public PasswordAgePolicy Age { get; set; }
    }

    public class PasswordExpirationPolicy
    {
        public int? PasswordExpireDays { get; set; }
    }

    public class PasswordComplexityPolicy
    {
        public int? MinLength { get; set; }
        public int? MinLowerCase { get; set; }
        public int? MinUpperCase { get; set; }
        public int? MinNumber { get; set; }
        public int? MinSymbol { get; set; }
        public string ExcludeUsername { get; set; }
    }

    public class PasswordAgePolicy
    {
        public int? MinAgeMinutes { get; set; }
        public int? HistoryCount { get; set; }
    }

    public class RecoveryTransactionLinks
    {
        public Link Next { get; set; }
        public Link Cancel { get; set; }
    }

    public class RecoveryTransactionObject
    {
        public string StateToken { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public string Status { get; set; }
        public string RelayState { get; set; }

        [JsonProperty("_embedded")]
        public RecoveryTransactionEmbedded Embedded { get; set; }

        [JsonProperty("_links")]
        public RecoveryTransactionLinks Links { get; set; }
    }

}
