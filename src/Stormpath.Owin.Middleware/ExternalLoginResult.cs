using Stormpath.SDK.Account;

namespace Stormpath.Owin.Middleware
{
    public class ExternalLoginResult
    {
        public IAccount Account { get; set; }

        public bool IsNewAccount { get; set; }
    }
}
