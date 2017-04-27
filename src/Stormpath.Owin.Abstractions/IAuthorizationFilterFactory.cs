using System.Collections.Generic;

namespace Stormpath.Owin.Abstractions
{
    public interface IAuthorizationFilterFactory
    {
        IAuthorizationFilter CreateGroupFilter(IEnumerable<string> allowedGroupNames);

        IAuthorizationFilter CreateCustomDataFilter(string key, object value, IEqualityComparer<object> comparer = null);
    }
}
