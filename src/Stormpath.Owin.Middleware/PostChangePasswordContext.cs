using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    public sealed class PostChangePasswordContext : HandlerContext
    {
        public PostChangePasswordContext(IOwinEnvironment environment, ICompatibleOktaAccount account)
            : base(environment)
        {
            Account = account;
        }

        public ICompatibleOktaAccount Account { get; }
    }
}
