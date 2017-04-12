﻿using Microsoft.Extensions.Logging;
using Stormpath.Owin.Middleware.Okta;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware
{
    internal static class UserHelper
    {
        public static async Task<dynamic> GetUserFromAccessTokenAsync(
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
            var stormpathCompatibleUser = new StormpathUserTransformer(logger).OktaToStormpathUser(oktaUser);
            return stormpathCompatibleUser;
        }
    }
}