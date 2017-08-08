namespace Stormpath.Owin.Middleware.Okta
{
    public interface IFriendlyErrorTranslator
    {
        string GetDefaultMessage();

        string GetFriendlyMessage(OktaException oex);
    }
}
