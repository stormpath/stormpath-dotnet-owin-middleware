using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    public sealed class PostLoginContext : HandlerContext
    {
        public PostLoginContext(IOwinEnvironment environment, ICompatibleOktaAccount account)
            : base(environment)
        {
            Account = account;
        }

        public ICompatibleOktaAccount Account { get; }

        public PostLoginResult Result { get; set; }
    }
}
