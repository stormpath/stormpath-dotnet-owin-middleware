using Stormpath.Owin.Abstractions;
namespace Stormpath.Owin.Middleware
{
    public class HandlerContext
    {
        public HandlerContext(IOwinEnvironment environment)
        {
            OwinEnvironment = environment;
        }

        public IOwinEnvironment OwinEnvironment { get; }
    }
}
