using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware.Internal
{
    internal sealed class AccountResponseSanitizer
    {
        public object Sanitize(ICompatibleOktaAccount account)
        {
            return new
            {
                account.Href,
                account.Username,
                account.ModifiedAt,
                Status = account.Status?.ToString(),
                account.CreatedAt,
                account.Email,
                account.MiddleName,
                account.Surname,
                account.GivenName,
                account.FullName
            };
        }
    }
}
