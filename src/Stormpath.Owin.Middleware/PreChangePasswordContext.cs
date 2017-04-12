using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class PreChangePasswordContext : HandlerContext
    {
        public PreChangePasswordContext(IOwinEnvironment environment, Okta.User user)
            : base(environment)
        {
            User = user;
        }

        public Okta.User User { get; }
    }
}
