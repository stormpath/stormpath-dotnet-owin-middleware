using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class PostChangePasswordContext : HandlerContext
    {
        public PostChangePasswordContext(IOwinEnvironment environment, Okta.User user)
            : base(environment)
        {
            User = user;
        }

        public Okta.User User { get; }
    }
}
