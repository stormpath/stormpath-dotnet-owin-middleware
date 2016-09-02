using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.SDK;
using Stormpath.SDK.Account;

namespace Stormpath.Owin.Middleware
{
    public sealed class RequiredGroupsFilter : IAuthorizationFilter
    {
        private readonly string[] _allowedGroups;

        public RequiredGroupsFilter(IEnumerable<string> allowedGroupNamesOrHrefs)
        {
            _allowedGroups = allowedGroupNamesOrHrefs.ToArray();

            if (_allowedGroups.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException("Group names or hrefs must not be null", nameof(allowedGroupNamesOrHrefs));
            }
        }

        public async Task<bool> IsAuthorized(IAccount account, CancellationToken cancellationToken)
        {
            if (account == null)
            {
                return false;
            }

            var matchedGroup = false;

            await account.GetGroups().ForEachAsync(item =>
                {
                    matchedGroup = _allowedGroups.Contains(item.Name, StringComparer.Ordinal)
                                   || _allowedGroups.Contains(item.Href, StringComparer.Ordinal);
                    return matchedGroup;
                },
                cancellationToken).ConfigureAwait(false);

            return matchedGroup;
        }
    }
}
