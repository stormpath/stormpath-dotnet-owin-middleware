using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    public sealed class PostLogoutContext : HandlerContext
    {
        public PostLogoutContext(IOwinEnvironment environment, ICompatibleOktaAccount account)
            : base(environment)
        {
            Account = account;
        }

        public ICompatibleOktaAccount Account { get; }
    }
}
