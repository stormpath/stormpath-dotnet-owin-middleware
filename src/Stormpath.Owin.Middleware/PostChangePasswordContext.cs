using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class PostChangePasswordContext : HandlerContext
    {
        public PostChangePasswordContext(IOwinEnvironment environment, dynamic account)
            : base(environment)
        {
            Account = account;
        }

        public dynamic Account { get; }
    }
}
