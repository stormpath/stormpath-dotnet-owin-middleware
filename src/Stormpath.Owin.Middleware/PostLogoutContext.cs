using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Account;

namespace Stormpath.Owin.Middleware
{
    public sealed class PostLogoutContext : HandlerContext
    {
        public PostLogoutContext(IOwinEnvironment environment, IAccount account)
            : base(environment)
        {
            Account = account;
        }

        public IAccount Account { get; }
    }
}
