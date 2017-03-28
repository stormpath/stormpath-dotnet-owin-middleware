using System;

namespace Stormpath.Owin.Middleware.Internal
{
    internal sealed class AccountResponseSanitizer
    {
        public object Sanitize(dynamic account)
        {
            // TODO
            throw new Exception("TODO");
            //return new
            //{
            //    account.Href,
            //    account.Username,
            //    account.ModifiedAt,
            //    Status = account.Status.ToString(),
            //    account.CreatedAt,
            //    account.Email,
            //    account.MiddleName,
            //    account.Surname,
            //    account.GivenName,
            //    account.FullName
            //};
        }
    }
}
