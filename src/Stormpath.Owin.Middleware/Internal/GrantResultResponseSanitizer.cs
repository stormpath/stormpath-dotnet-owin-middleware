using Stormpath.SDK.Oauth;

namespace Stormpath.Owin.Middleware.Internal
{
    public class GrantResultResponseSanitizer
    {
        public object SanitizeResponseWithRefreshToken(IOauthGrantAuthenticationResult result)
        {
            return new
            {
                access_token = result.AccessTokenString,
                expires_in = result.ExpiresIn,
                refresh_token = result.RefreshTokenString,
                token_type = result.TokenType
            };
        }

        public object SanitizeResponseWithoutRefreshToken(IOauthGrantAuthenticationResult result)
        {
            return new
            {
                access_token = result.AccessTokenString,
                expires_in = result.ExpiresIn,
                token_type = result.TokenType
            };
        }
    }
}
