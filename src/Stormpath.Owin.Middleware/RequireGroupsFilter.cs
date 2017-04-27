using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    public sealed class RequireGroupsFilter : IAuthorizationFilter
    {
        private readonly IOktaClient _oktaClient;
        private readonly string[] _allowedGroups;

        public RequireGroupsFilter(IOktaClient oktaClient, IEnumerable<string> allowedGroupNames)
        {
            _oktaClient = oktaClient;
            _allowedGroups = allowedGroupNames.ToArray();

            if (_allowedGroups.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException("Group names or hrefs must not be null", nameof(allowedGroupNames));
            }
        }

        public bool IsAuthorized(ICompatibleOktaAccount account)
        {
            var userId = account?.GetOktaUser()?.Id;
            if (string.IsNullOrEmpty(userId)) return false;

            var groups = _oktaClient.GetGroupsForUserIdAsync(userId, CancellationToken.None).Result;
            return groups.Any(x => _allowedGroups.Contains(x?.Profile?.Name, StringComparer.Ordinal));
        }

        public async Task<bool> IsAuthorizedAsync(ICompatibleOktaAccount account, CancellationToken cancellationToken)
        {
            var userId = account?.GetOktaUser()?.Id;
            if (string.IsNullOrEmpty(userId)) return false;

            var groups = await _oktaClient.GetGroupsForUserIdAsync(userId, cancellationToken);
            return groups.Any(x => _allowedGroups.Contains(x?.Profile?.Name, StringComparer.Ordinal));
        }
    }
}
