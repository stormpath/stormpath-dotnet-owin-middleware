using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace Stormpath.Owin.Middleware
{
    public sealed class StateTokenParser
    {
        public StateTokenParser(string appId, string secret, string token, ILogger logger)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrEmpty(secret))
            {
                throw new ArgumentNullException(nameof(secret));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            DecodeToken(appId, secret, token, logger);
        }

        public string Path { get; private set; }

        public string State { get; private set; }

        public bool Valid { get; private set; } = false;

        private void DecodeToken(string appId, string secret, string token, ILogger logger)
        {
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            try
            {
                var signingCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)), SecurityAlgorithms.HmacSha256);

                var validationParameters = new TokenValidationParameters()
                {
                    //ClockSkew = ?? todo
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidAudience = appId,
                    IssuerSigningKey = signingCredentials.Key
                };

                new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out var validToken);
                var validJwtToken = (JwtSecurityToken)validToken;

                if (!validJwtToken.SignatureAlgorithm.Equals("HS256", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("State token has an invalid signature algorithm: {0}", token);
                    return;
                }

                var pathClaim = validJwtToken.Claims.SingleOrDefault(c => c.Type == StateTokenBuilder.PathClaimName);
                if (pathClaim != null)
                {
                    Path = pathClaim.Value;
                }

                var innerStateClaim = validJwtToken.Claims.SingleOrDefault(c => c.Type == StateTokenBuilder.StateClaimName);
                if (innerStateClaim != null)
                {
                    State = innerStateClaim.Value;
                }

                Valid = true;
                return;
            }
            catch (SecurityTokenExpiredException)
            {
                logger.LogWarning("State token has expired: {0}", token);
                return;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                logger.LogWarning("State token has invalid signature: {0}", token);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Unexpected error '{0}' while validating state token: {1}", ex.Message, token);
                return;
            }
        }
    }
}
