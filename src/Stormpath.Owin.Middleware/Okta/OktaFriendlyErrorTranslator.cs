using System;
using System.Collections.Generic;
using System.Linq;

namespace Stormpath.Owin.Middleware.Okta
{
    public sealed class OktaFriendlyErrorTranslator : IFriendlyErrorTranslator
    {
        private const string DefaultMessage = "Sorry, an error has occurred. Please try again.";

        private const string ValidationErrorCode = "E0000001";
        private const string PasswordRequirementsErrorCode = "E0000080";
        private const string InvalidGrant = "invalid_grant";

        private static string GetCodeFromException(OktaException oex)
        {
            if (oex == null)
            {
                return null;
            }

            oex.Body.TryGetValue("errorCode", out var rawErrorCode);
            return rawErrorCode?.ToString();
        }

        private static string GetFirstErrorCause(OktaException oex)
        {
            if (oex == null)
            {
                return null;
            }

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

        private static bool TryHandleOauthError(OktaException oex, out string message)
        {
            message = null;

            if (oex == null)
            {
                return false;
            }

            oex.Body.TryGetValue("error", out var rawError);
            var error = rawError?.ToString();

            if (string.IsNullOrEmpty(error))
            {
                return false;
            }

            if (error.Equals(InvalidGrant, StringComparison.Ordinal))
            {
                message = "Invalid username or password.";
                return true;
            }

            return false;
        }

        private static bool TryHandleValidationError(OktaException oex, out string message)
        {
            message = null;

            var code = GetCodeFromException(oex);

            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            var cause = GetFirstErrorCause(oex);

            if (code.Equals(ValidationErrorCode, StringComparison.Ordinal))
            {
                if (cause.StartsWith("Password requirements were not met", StringComparison.Ordinal))
                {
                    message = cause;
                    return true;
                }
            }

            if (code.Equals(PasswordRequirementsErrorCode, StringComparison.Ordinal))
            {
                message = cause ?? "Password requirements were not met.";
                return true;
            }

            return false;
        }

        public string GetDefaultMessage() => DefaultMessage;

        public string GetFriendlyMessage(OktaException oex)
        {
            if (TryHandleOauthError(oex, out var oauthErrorMessage))
            {
                return oauthErrorMessage;
            }

            if (TryHandleValidationError(oex, out var validationErrorMessage))
            {
                return validationErrorMessage;
            }

            return DefaultMessage;
        }
    }
}
