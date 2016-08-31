using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public class PreVerifyEmailContext : HandlerContext
    {
        public PreVerifyEmailContext(IOwinEnvironment environment)
            : base(environment)
        {
        }

        public string Email { get; set; }
    }
}
