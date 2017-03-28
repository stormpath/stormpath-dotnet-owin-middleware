using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class PreChangePasswordContext : HandlerContext
    {
        public PreChangePasswordContext(IOwinEnvironment environment, dynamic account)
            : base(environment)
        {
            Account = account;
        }

        public dynamic Account { get; }
    }
}
