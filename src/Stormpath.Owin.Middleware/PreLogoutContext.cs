using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class PreLogoutContext : HandlerContext
    {
        public PreLogoutContext(IOwinEnvironment environment, dynamic account)
            : base(environment)
        {
            Account = account;
        }

        public dynamic Account { get; }
    }
}
