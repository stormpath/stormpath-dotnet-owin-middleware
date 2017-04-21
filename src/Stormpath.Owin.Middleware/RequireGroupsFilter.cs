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
        private readonly string[] _allowedGroups;

        public RequireGroupsFilter(IEnumerable<string> allowedGroupNamesOrHrefs)
        {
            _allowedGroups = allowedGroupNamesOrHrefs.ToArray();

            if (_allowedGroups.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException("Group names or hrefs must not be null", nameof(allowedGroupNamesOrHrefs));
            }
        }

        public bool IsAuthorized(ICompatibleOktaAccount account)
        {
            if (account == null)
            {
                return false;
            }

            // todo rewrite group authz logic
            throw new Exception("TODO");

            //var matchedGroup = false;

            //foreach (var group in account.GetGroups().Synchronously())
            //{
            //    matchedGroup = _allowedGroups.Contains(group.Name, StringComparer.Ordinal)
            //        || _allowedGroups.Contains(group.Href, StringComparer.Ordinal);

            //    if (matchedGroup)
            //    {
            //        break;
            //    }
            //}

            //return matchedGroup;
        }

        [Obsolete("Use the synchronous IsAuthorized")]
        public Task<bool> IsAuthorizedAsync(ICompatibleOktaAccount account, CancellationToken cancellationToken)
        {
            if (account == null)
            {
                return Task.FromResult(false);
            }

            // todo rewrite group authz logic
            throw new Exception("TODO");

            //var matchedGroup = false;

            //await account.GetGroups().ForEachAsync(group =>
            //    {
            //        matchedGroup = _allowedGroups.Contains(group.Name, StringComparer.Ordinal)
            //                       || _allowedGroups.Contains(group.Href, StringComparer.Ordinal);
            //        return matchedGroup;
            //    },
            //    cancellationToken).ConfigureAwait(false);

            //return matchedGroup;
        }
    }
}
