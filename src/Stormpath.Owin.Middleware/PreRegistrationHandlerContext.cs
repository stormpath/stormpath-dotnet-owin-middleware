using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Account;
using Stormpath.SDK.Directory;

namespace Stormpath.Owin.Middleware
{
    public class PreRegistrationHandlerContext : HandlerContext
    {
        public PreRegistrationHandlerContext(IOwinEnvironment environment, IAccount account)
            : base(environment)
        {
            Account = account;
        }

        public IAccount Account { get; }

        public IDirectory AccountStore { get; set; }

        public IAccountCreationOptions Options { get; set; }
    }
}
