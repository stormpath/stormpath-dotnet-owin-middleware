namespace Stormpath.Owin.Middleware.Internal
{
    public class GrantResultResponseSanitizer
    {
        public object SanitizeResponseWithRefreshToken(GrantResult result)
        {
            return new
            {
                access_token = result.AccessToken,
                expires_in = result.ExpiresIn,
                refresh_token = result.RefreshToken,
                token_type = result.TokenType
            };
        }

        public object SanitizeResponseWithoutRefreshToken(GrantResult result)
        {
            return new
            {
                access_token = result.AccessToken,
                expires_in = result.ExpiresIn,
                token_type = result.TokenType
            };
        }
    }
}
