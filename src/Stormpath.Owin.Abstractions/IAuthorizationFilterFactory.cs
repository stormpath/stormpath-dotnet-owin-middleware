using System.Collections.Generic;

namespace Stormpath.Owin.Abstractions
{
    public interface IAuthorizationFilterFactory
    {
        IAuthorizationFilter CreateGroupFilter(IEnumerable<string> allowedGroupNames);
    }
}
