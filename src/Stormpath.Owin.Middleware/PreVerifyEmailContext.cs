using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class PreVerifyEmailContext : HandlerContext
    {
        public PreVerifyEmailContext(IOwinEnvironment environment)
            : base(environment)
        {
        }

        public string Email { get; set; }
    }
}
