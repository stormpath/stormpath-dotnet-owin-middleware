using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Client;

namespace Stormpath.Owin.Middleware
{
    public class HandlerContext
    {
        public HandlerContext(IOwinEnvironment environment)
        {
            OwinEnvironment = environment;
        }

        public IOwinEnvironment OwinEnvironment { get; }

        public IClient Client => OwinEnvironment.Request[OwinKeys.StormpathClient] as IClient;
    }
}
