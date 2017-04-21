using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Okta;

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

        public bool IsAuthorized(ICompatibleOktaAccount account)
            => _comparer.Equals(account?.CustomData[_key], _value);

        [Obsolete("Use the synchronous IsAuthorized")]
        public Task<bool> IsAuthorizedAsync(ICompatibleOktaAccount account, CancellationToken cancellationToken)
            => Task.FromResult(IsAuthorized(account));
    }
}
