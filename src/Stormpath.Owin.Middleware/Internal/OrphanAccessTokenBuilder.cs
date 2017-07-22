using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Stormpath.Owin.Abstractions.Configuration;

namespace Stormpath.Owin.Middleware.Internal
{
    internal sealed class OrphanAccessTokenBuilder
    {
        private readonly IntegrationConfiguration _configuration;

        public OrphanAccessTokenBuilder(IntegrationConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Build(string id, string userId, int timeToLive)
        {
            var signingKey = _configuration.OktaEnvironment.ClientSecret;

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;

            var claims = new[]
            {
                new Claim("sub", id),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("iat", ((long)((now - Cookies.Epoch).TotalSeconds)).ToString(), ClaimValueTypes.Integer64),
                new Claim("cid", _configuration.OktaEnvironment.ClientId),
                new Claim("uid", userId)
            };

            var jwt = new JwtSecurityToken(
                claims: claims,
                issuer: _configuration.Application.Id,
                expires: now + TimeSpan.FromSeconds(timeToLive),
                notBefore: DateTime.UtcNow,
                signingCredentials: signingCredentials,
                audience: _configuration.OktaEnvironment.ClientId);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
