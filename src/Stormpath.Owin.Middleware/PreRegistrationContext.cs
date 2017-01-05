using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Account;
using Stormpath.SDK.Directory;

namespace Stormpath.Owin.Middleware
{
    public sealed class PreRegistrationContext : HandlerContext
    {
        public PreRegistrationContext(IOwinEnvironment environment, IAccount account)
            : base(environment)
        {
            Account = account;
        }

        public IAccount Account { get; }

        public IDirectory AccountStore { get; set; }

        public IAccountCreationOptions Options { get; set; }

        public PreRegistrationResult Result { get; set; }
    }
}
