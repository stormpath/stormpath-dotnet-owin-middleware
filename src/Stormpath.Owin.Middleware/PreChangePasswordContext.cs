using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Account;

namespace Stormpath.Owin.Middleware
{
    public sealed class PreChangePasswordContext : HandlerContext
    {
        public PreChangePasswordContext(IOwinEnvironment environment, IAccount account)
            : base(environment)
        {
            Account = account;
        }

        public IAccount Account { get; }
    }
}
