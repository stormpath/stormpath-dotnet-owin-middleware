using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    internal sealed class ExternalLoginResult
    {
        public ICompatibleOktaAccount Account { get; set; }

        public bool IsNewAccount { get; set; }
    }
}
