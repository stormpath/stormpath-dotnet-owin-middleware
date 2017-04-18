using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware.Okta
{
    public sealed class CompatibleOktaAccount : ICompatibleOktaAccount
    {
        private readonly User _oktaUser;

        public const string AccountEnabled = "ENABLED";
        public const string AccountDisabled = "DISABLED";
        public const string AccountUnverified = "UNVERIFIED";

        private static IReadOnlyDictionary<string, string> StatusMap = 
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ACTIVE"] = AccountEnabled,
            ["DEPROVISIONED"] = AccountDisabled,
            ["LOCKED_OUT"] = AccountDisabled,
            ["PASSWORD_EXPIRED"] = AccountDisabled,
            ["PROVISIONED"] = AccountUnverified,
            ["RECOVERY"] = AccountEnabled,
            ["STAGED"] = AccountUnverified,
            ["SUSPENDED"] = AccountDisabled
        };

        private static IReadOnlyDictionary<string, string> OktaProfileMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["login"] = "Username",
            ["email"] = "Email",
            ["firstName"] = "GivenName",
            ["middleName"] = "MiddleName",
            ["lastName"] = "Surname",
            ["emailVerificationStatus"] = "EmailVerificationStatus",
            ["emailVerificationToken"] = "EmailVerificationToken"
        };

        public CompatibleOktaAccount(User oktaUser)
        {
            _oktaUser = oktaUser ?? new User();
        }

        public User OktaUser => _oktaUser;

        private string GetStringOrDefault(string key)
            => _oktaUser.Profile?.GetOrDefault(key)?.ToString();

        private void SetString(string key, object value)
            => _oktaUser.Profile[key] = value;

        public string Status => StatusMap.TryGetValue(_oktaUser?.Status, out var mappedStatus) ? mappedStatus : "UNKNOWN";

        public string Href => _oktaUser.Links?.Self?.Href;

        public DateTimeOffset? CreatedAt => _oktaUser.Created;

        public DateTimeOffset? ModifiedAt => _oktaUser.LastUpdated;

        public DateTimeOffset? PasswordModifiedAt => _oktaUser.PasswordChanged;

        public string FullName => string.Join(" ", GivenName, MiddleName, Surname);

        public string GivenName
        {
            get => GetStringOrDefault("firstName");
            set => SetString("firstName", value);
        }

        public string MiddleName
        {
            get => GetStringOrDefault("middleName");
            set => SetString("middleName", value);
        }

        public string Surname
        {
            get => GetStringOrDefault("lastName");
            set => SetString("lastName", value);
        }

        public string Username
        {
            get => GetStringOrDefault("login");
            set => SetString("login", value);
        }

        public string Email
        {
            get => GetStringOrDefault("email");
            set => SetString("email", value);
        }

        public string EmailVerificationStatus => GetStringOrDefault("emailVerificationStatus");

        public string EmailVerificationToken => GetStringOrDefault("emailVerificationToken");

        // TODO: filter
        public IDictionary<string, object> CustomData => _oktaUser.Profile;

        public IDictionary<string, object> GetCustomData() => CustomData;

        public Task<IDictionary<string, object>> GetCustomDataAsync(CancellationToken _)
            => Task.FromResult(CustomData);
    }
}
