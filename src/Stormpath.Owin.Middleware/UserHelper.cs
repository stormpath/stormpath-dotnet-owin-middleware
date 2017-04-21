using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    internal static class UserHelper
    {
        public static async Task<ICompatibleOktaAccount> GetUserFromAccessTokenAsync(
            IOktaClient oktaClient,
            string accessToken,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            if (!token.Payload.TryGetValue("uid", out object rawUid))
            {
                throw new Exception("Could not get user information");
            }

            var oktaUser = await oktaClient.GetUserAsync(rawUid.ToString(), cancellationToken);
            return new CompatibleOktaAccount(oktaUser);
        }
    }
}
