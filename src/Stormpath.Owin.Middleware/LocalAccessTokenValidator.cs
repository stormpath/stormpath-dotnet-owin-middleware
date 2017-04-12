using Microsoft.IdentityModel.Tokens;
using Stormpath.Owin.Middleware.Okta;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware
{
    public sealed class LocalAccessTokenValidator
    {
        private readonly IOktaClient _oktaClient;
        private readonly IKeyProvider _keyProvider;
        private readonly string _orgHref;
        private readonly string _authorizationServerId;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public LocalAccessTokenValidator(
            IOktaClient oktaClient,
            IKeyProvider keyProvider,
            string orgHref,
            string authorizationServerId,
            string clientId,
            string clientSecret)
        {
            _oktaClient = oktaClient;
            _keyProvider = keyProvider;

            _orgHref = orgHref;
            _authorizationServerId = authorizationServerId;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task<JwtSecurityToken> ValidateSecurityTokenAsync(string token, CancellationToken cancellationToken)
        {
            // TODO need ITs for this

            var param = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = $"{_orgHref}/oauth2/{_authorizationServerId}",
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeyResolver = await _keyProvider.GetSigningKeyResolver(cancellationToken),

                // TODO what will the standard audience be?
                ValidateAudience = false
            };

            new JwtSecurityTokenHandler().ValidateToken(token, param, out SecurityToken securityToken);

            return securityToken as JwtSecurityToken;
        }

        public async Task<TokenIntrospectionResult> ValidateAsync(string token, CancellationToken cancellationToken)
        {
            var decodedToken = await ValidateSecurityTokenAsync(token, cancellationToken);
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
