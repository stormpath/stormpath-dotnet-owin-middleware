using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Account;

namespace Stormpath.Owin.Middleware
{
    public sealed class RequireCustomDataFilter : IAuthorizationFilter
    {
        private readonly string _key;
        private readonly object _value;

        public RequireCustomDataFilter(string key, object value)
        {
            _key = key;
            _value = value;
        }

        public async Task<bool> IsAuthorized(IAccount account, CancellationToken cancellationToken)
        {
            if (account == null)
            {
                return false;
            }

            var customData = await account.GetCustomDataAsync(cancellationToken);

            if (customData?[_key] == null)
            {
                return false;
            }

            return customData[_key].Equals(_value);
        }
    }
}
