using Stormpath.Owin.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware
{
    public sealed class RequireCustomDataFilter : IAuthorizationFilter
    {
        private readonly string _key;
        private readonly object _value;
        private readonly IEqualityComparer<object> _comparer;

        public RequireCustomDataFilter(string key, object value, IEqualityComparer<object> comparer = null)
        {
            _key = key;
            _value = value;
            _comparer = comparer ?? new DefaultSmartComparer();
        }

        public bool IsAuthorized(ICompatibleOktaAccount account)
        {
            object rawValue = null;

            bool exists = account?.CustomData?.TryGetValue(_key, out rawValue) ?? false;
            if (!exists) return false;

            return _comparer.Equals(rawValue, _value);
        }

        public Task<bool> IsAuthorizedAsync(ICompatibleOktaAccount account, CancellationToken cancellationToken)
            => Task.FromResult(IsAuthorized(account));
    }
}
