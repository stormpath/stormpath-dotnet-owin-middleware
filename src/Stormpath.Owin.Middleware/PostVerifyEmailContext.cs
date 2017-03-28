using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class PostVerifyEmailContext : HandlerContext
    {
        public PostVerifyEmailContext(IOwinEnvironment environment, dynamic account)
            : base(environment)
        {
            Account = account;
        }

        public dynamic Account { get; }
    }
}
