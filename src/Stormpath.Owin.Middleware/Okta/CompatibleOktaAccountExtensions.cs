using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware.Okta
{
    public static class CompatibleOktaAccountExtensions
    {
        public static User GetOktaUser(this ICompatibleOktaAccount account)
            => (account as IHasOktaUser)?.GetOktaUser();
    }
}
