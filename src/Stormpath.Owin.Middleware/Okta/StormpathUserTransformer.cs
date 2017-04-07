using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware.Okta
{
    public class StormpathUserTransformer
    {
        public const string AccountEnabled = "ENABLED";
        public const string AccountDisabled = "DISABLED";
        public const string AccountUnverified = "UNVERIFIED";

        private static IReadOnlyDictionary<string, string> StatusMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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

        private static IReadOnlyDictionary<string, string> EmailVerificationStatusMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ACTIVE"] = "VERIFIED",
            ["DEPROVISIONED"] = "UNKNOWN",
            ["LOCKED_OUT"] = "VERIFIED",
            ["PASSWORD_EXPIRED"] = "VERIFIED",
            ["PROVISIONED"] = "UNVERIFIED",
            ["RECOVERY"] = "VERIFIED",
            ["STAGED"] = "UNVERIFIED",
            ["SUSPENDED"] = "VERIFIED"
        };

        private static IReadOnlyDictionary<string, string> OktaProfileMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["login"] = "Username",
            ["email"] = "Email",
            ["firstName"] = "GivenName",
            ["middleName"] = "MiddleName",
            ["lastName"] = "Surname",
            ["emailVerificationStatus"] = "EmailVerificationStatus"
        };

        private readonly ILogger _logger;

        public StormpathUserTransformer(ILogger logger)
        {
            _logger = logger;
        }

        public dynamic OktaToStormpathUser(User oktaUser)
        {
            // TODO unit tests for this 

            if (oktaUser == null)
            {
                return new ExpandoObject();
            }

            dynamic stormpathAccount = oktaUser.ToDynamic();

            // Guarantee some properties (to avoid RuntimeBinderExceptions)
            stormpathAccount.Href = oktaUser.Links?.Self?.Href;
            stormpathAccount.Status = StatusMap.TryGetValue(oktaUser.Status, out var mappedStatus) ? mappedStatus : "UNKNOWN";
            stormpathAccount.CreatedAt = oktaUser.Created;
            stormpathAccount.ModifiedAt = oktaUser.LastUpdated;
            stormpathAccount.PasswordModifiedAt = oktaUser.PasswordChanged;
            stormpathAccount.FullName = null;
            stormpathAccount.GivenName = null;
            stormpathAccount.MiddleName = null;
            stormpathAccount.Surname = null;
            stormpathAccount.Username = null;
            stormpathAccount.Email = null;
            stormpathAccount.EmailVerificationToken = null;

            try
            {
                stormpathAccount.FullName = $"{stormpathAccount.Profile.firstName} {stormpathAccount.Profile.lastName}";

                var customData = new ExpandoObject() as IDictionary<string, object>;
                var stormpathAccountAsDictionary = stormpathAccount as IDictionary<string, object>;
                var profileAsDictionary = stormpathAccount.Profile as IDictionary<string, object>;
                foreach (var key in profileAsDictionary.Keys)
                {
                    if (OktaProfileMap.TryGetValue(key, out var stormpathKey))
                    {
                        stormpathAccountAsDictionary[stormpathKey] = profileAsDictionary[key];
                    }
                    else
                    {
                        customData[key] = profileAsDictionary[key];
                    }
                }

                stormpathAccount.CustomData = customData as ExpandoObject;
                stormpathAccount.GetCustomDataAsync = new Func<CancellationToken, Task<dynamic>>((_) => Task.FromResult<dynamic>(stormpathAccount.CustomData));
                stormpathAccount.GetCustomData = new Func<dynamic>(() => stormpathAccount.CustomData);

                return stormpathAccount;
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException rbex)
            {
                _logger.LogWarning(1000, rbex, "Could not transform Okta profile, returning default");
                return stormpathAccount;
            }
        }
    }
}
