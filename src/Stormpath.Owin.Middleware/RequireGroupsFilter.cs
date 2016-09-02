using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.SDK;
using Stormpath.SDK.Account;
using Stormpath.SDK.Sync;

namespace Stormpath.Owin.Middleware
{
    public sealed class RequireGroupsFilter : IAuthorizationFilter
    {
        private readonly string[] _allowedGroups;

        public RequireGroupsFilter(IEnumerable<string> allowedGroupNamesOrHrefs)
        {
            _allowedGroups = allowedGroupNamesOrHrefs.ToArray();

            if (_allowedGroups.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException("Group names or hrefs must not be null", nameof(allowedGroupNamesOrHrefs));
            }
        }

        public bool IsAuthorized(IAccount account)
        {
            if (account == null)
            {
                return false;
            }

            var matchedGroup = false;

            foreach (var group in account.GetGroups().Synchronously())
            {
                matchedGroup = _allowedGroups.Contains(group.Name, StringComparer.Ordinal)
                    || _allowedGroups.Contains(group.Href, StringComparer.Ordinal);

                if (matchedGroup)
                {
                    break;
                }
            }

            return matchedGroup;
        }

        public async Task<bool> IsAuthorizedAsync(IAccount account, CancellationToken cancellationToken)
        {
            if (account == null)
            {
                return false;
            }

            var matchedGroup = false;

            await account.GetGroups().ForEachAsync(group =>
                {
                    matchedGroup = _allowedGroups.Contains(group.Name, StringComparer.Ordinal)
                                   || _allowedGroups.Contains(group.Href, StringComparer.Ordinal);
                    return matchedGroup;
                },
                cancellationToken).ConfigureAwait(false);

            return matchedGroup;
        }
    }
}
