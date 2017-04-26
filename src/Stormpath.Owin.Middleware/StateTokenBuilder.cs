using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Stormpath.Owin.Middleware
{
    public sealed class StateTokenBuilder
    {
        public const string PathClaimName = "path";
        public const string StateClaimName = "state";

        private readonly string _secret;
        private readonly string _appId;

        public StateTokenBuilder(string appId, string secret)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            if (string.IsNullOrEmpty(secret))
            {
                throw new ArgumentNullException(nameof(secret));
            }

            _appId = appId;
            _secret = secret;
        }

        public string Path { get; set; }

        public string State { get; set; } = Guid.NewGuid().ToString();

        public TimeSpan ExpiresIn { get; set; } = TimeSpan.FromMinutes(30);

        public IList<Claim> Claims { get; set; } = new List<Claim>();

        public override string ToString()
        {
            var expiry = DateTime.UtcNow.Add(ExpiresIn);

            var customClaims = new List<Claim>();

            if (!string.IsNullOrEmpty(Path))
            {
                customClaims.Add(new Claim(PathClaimName, Path));
            }

            if (!string.IsNullOrEmpty(State))
            {
                customClaims.Add(new Claim(StateClaimName, State));
            }

            if (Claims != null)
            {
                customClaims.AddRange(Claims);
            }

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secret)), SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                claims: customClaims,
                expires: expiry,
                notBefore: DateTime.UtcNow,
                audience: _appId,
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
