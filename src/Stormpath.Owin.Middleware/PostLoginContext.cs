using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed class PostLoginContext : HandlerContext
    {
        public PostLoginContext(IOwinEnvironment environment, dynamic account)
            : base(environment)
        {
            Account = account;
        }

        public dynamic Account { get; }

        public PostLoginResult Result { get; set; }
    }
}
