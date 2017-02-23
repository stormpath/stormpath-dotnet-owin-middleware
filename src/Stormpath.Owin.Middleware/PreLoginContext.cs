using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class PreLoginContext : HandlerContext
    {
        public PreLoginContext(IOwinEnvironment environment)
            : base(environment)
        {
        }

        public string Login { get; set; }

        public PreLoginResult Result { get; set; }
    }
}
