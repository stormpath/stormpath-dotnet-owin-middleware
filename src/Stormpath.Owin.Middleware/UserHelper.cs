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
        public static async Task<User> GetUserFromAccessTokenAsync(
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

            return await oktaClient.GetUserAsync(rawUid.ToString(), cancellationToken);
        }

        public static async Task<ICompatibleOktaAccount> GetAccountFromAccessTokenAsync(
            IOktaClient oktaClient,
            string accessToken,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            return new CompatibleOktaAccount(
                await GetUserFromAccessTokenAsync(oktaClient, accessToken, logger, cancellationToken));
        }
    }
}
