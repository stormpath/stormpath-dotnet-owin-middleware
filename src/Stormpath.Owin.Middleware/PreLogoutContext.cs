using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    public sealed class PreLogoutContext : HandlerContext
    {
        public PreLogoutContext(IOwinEnvironment environment, ICompatibleOktaAccount account)
            : base(environment)
        {
            Account = account;
        }

        public ICompatibleOktaAccount Account { get; }
    }
}
