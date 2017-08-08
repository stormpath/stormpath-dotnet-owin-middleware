using System;
using System.Collections.Generic;
using System.Linq;

namespace Stormpath.Owin.Middleware.Okta
{
    public sealed class OktaFriendlyErrorTranslator : IFriendlyErrorTranslator
    {
        private const string DefaultMessage = "Sorry, an error has occurred. Please try again.";
        private const string ValidationErrorCode = "E0000001";

        private static string GetCodeFromException(OktaException oex)
        {
            oex.Body.TryGetValue("errorCode", out var rawErrorCode);
            return rawErrorCode?.ToString();
        }

        private static string GetFirstErrorCause(OktaException oex)
        {
            if (!oex.Body.TryGetValue("errorCauses", out var rawErrorCauses))
            {
                return null;
            }

            var firstCause =
                ((rawErrorCauses as IEnumerable<IEnumerable<object>>)?.FirstOrDefault() // Unwrap the array
                ?.FirstOrDefault() as IEnumerable<object>) // First child item
                ?.FirstOrDefault()?.ToString(); // First value (string)

            return firstCause;
        }

        public string GetDefaultMessage() => DefaultMessage;

        public string GetFriendlyMessage(OktaException oex)
        {
            var code = GetCodeFromException(oex);

            if (!code.Equals(ValidationErrorCode, StringComparison.Ordinal))
            {
                return DefaultMessage;
            }

            var cause = GetFirstErrorCause(oex);

            if (cause.StartsWith("Password requirements were not met", StringComparison.Ordinal))
            {
                return cause;
            }

            return DefaultMessage;
        }
    }
}
