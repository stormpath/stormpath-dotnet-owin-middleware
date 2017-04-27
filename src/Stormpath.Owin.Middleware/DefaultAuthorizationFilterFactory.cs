using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Okta;
using System.Collections.Generic;

namespace Stormpath.Owin.Middleware
{
    public sealed class DefaultAuthorizationFilterFactory : IAuthorizationFilterFactory
    {
        private readonly IOktaClient _oktaClient;

        public DefaultAuthorizationFilterFactory(IOktaClient oktaClient)
        {
            _oktaClient = oktaClient;
        }

        public IAuthorizationFilter CreateGroupFilter(IEnumerable<string> allowedGroupNames)
            => new RequireGroupsFilter(_oktaClient, allowedGroupNames);
    }
}
