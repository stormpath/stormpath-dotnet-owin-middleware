using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Account;

namespace Stormpath.Owin.Middleware
{
    public class PostVerifyEmailContext : HandlerContext
    {
        public PostVerifyEmailContext(IOwinEnvironment environment, IAccount account)
            : base(environment)
        {
            Account = account;
        }

        public IAccount Account { get; }
    }
}
