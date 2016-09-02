using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Account;

namespace Stormpath.Owin.Middleware
{
    public class PostLoginContext : HandlerContext
    {
        public PostLoginContext(IOwinEnvironment environment, IAccount account)
            : base(environment)
        {
            Account = account;
        }

        public IAccount Account { get; }
    }
}
