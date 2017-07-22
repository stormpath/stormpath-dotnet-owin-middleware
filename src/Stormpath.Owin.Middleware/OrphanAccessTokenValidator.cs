using Microsoft.IdentityModel.Tokens;
using Stormpath.Owin.Middleware.Okta;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace Stormpath.Owin.Middleware
{
    /// <summary>
    /// Validates tokens issued locally by this middleware code.
    /// This flow is used by the Client Credentials grant type.
    /// </summary>
    public sealed class OrphanAccessTokenValidator
    {
        private readonly string _applicationId;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public OrphanAccessTokenValidator(
            string applicationId,
            string clientId,
            string clientSecret)
        {
            _applicationId = applicationId;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public JwtSecurityToken ValidateSecurityToken(string token)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_clientSecret));

            var param = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = _applicationId,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateAudience = true,
                ValidAudience = _clientId,
            };

            try
            {
                new JwtSecurityTokenHandler().ValidateToken(token, param, out SecurityToken securityToken);

                return securityToken as JwtSecurityToken;
            }
            catch (Exception)
            {
                // Token is invalid
                return null;
            }
        }

        public TokenIntrospectionResult ValidateAsync(string token)
        {
            var decodedToken = ValidateSecurityToken(token);
            if (decodedToken == null) return TokenIntrospectionResult.Invalid;

            bool hasClientIdClaim = decodedToken.Payload.TryGetValue("cid", out var rawCid);
            if (!hasClientIdClaim) return TokenIntrospectionResult.Invalid;

            bool clientIdMatches = rawCid?.ToString().Equals(_clientId) ?? false;
            if (!clientIdMatches) return TokenIntrospectionResult.Invalid;

            decodedToken.Payload.TryGetValue("uid", out var rawUid);

            decodedToken.Payload.TryGetValue("scp", out var rawScope);
            var scopesAsArray = (rawScope as Newtonsoft.Json.Linq.JArray)?.Select(t => t?.ToString()) ?? new[] { string.Empty };

            return new TokenIntrospectionResult
            {
                Active = true,
                Aud = decodedToken.Payload.Aud,
                ClientId = rawCid.ToString(),
                Exp = decodedToken.Payload.Exp,
                Iat = decodedToken.Payload.Iat,
                Iss = decodedToken.Payload.Iss,
                Jti = decodedToken.Payload.Jti,
                Scope = string.Join(" ", scopesAsArray),
                Sub = decodedToken.Payload.Sub,
                TokenType = "Bearer",
                Uid = rawUid?.ToString(),
                Username = decodedToken.Payload.Sub
            };
        }

    }
}
