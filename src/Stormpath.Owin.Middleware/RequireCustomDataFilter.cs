using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class RequireCustomDataFilter : IAuthorizationFilter
    {
        private readonly string _key;
        private readonly object _value;
        private readonly IEqualityComparer<object> _comparer;

        public RequireCustomDataFilter(string key, object value)
            : this(key, value, new DefaultSmartComparer())
        {
        }

        public RequireCustomDataFilter(string key, object value, IEqualityComparer<object> comparer)
        {
            _key = key;
            _value = value;
            _comparer = comparer ?? new DefaultSmartComparer();
        }

        public bool IsAuthorized(dynamic account)
        {
            var customData = account?.GetCustomData();

            return _comparer.Equals(customData?[_key], _value);
        }

        public async Task<bool> IsAuthorizedAsync(dynamic account, CancellationToken cancellationToken)
        {
            var customData = account == null
                ? null
                : await account.GetCustomDataAsync(cancellationToken).ConfigureAwait(false);

            return _comparer.Equals(customData?[_key], _value);
        }
    }
}
