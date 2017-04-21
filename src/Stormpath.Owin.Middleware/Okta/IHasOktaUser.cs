namespace Stormpath.Owin.Middleware.Okta
{
    public interface IHasOktaUser
    {
        User GetOktaUser();
    }
}
