using Stormpath.Owin.Abstractions;
using Stormpath.SDK.AccountStore;

namespace Stormpath.Owin.Middleware
{
    public sealed class PreLoginContext : HandlerContext
    {
        public PreLoginContext(IOwinEnvironment environment)
            : base(environment)
        {
        }

        public string Login { get; set; }

        public IAccountStore AccountStore { get; set; }

        public PreLoginResult Result { get; set; }
    }
}
