﻿// <copyright file="StormpathMiddleware.GetUser.cs" company="Stormpath, Inc.">
// Copyright (c) 2016 Stormpath, Inc.
// Portions copyright 2013 Microsoft Open Technologies, Inc. Licensed under Apache 2.0.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Configuration.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public sealed partial class StormpathMiddleware
    {
        private async Task<dynamic> GetUserAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var bearerAuthenticationResult = await TryBearerAuthenticationAsync(context);
            if (bearerAuthenticationResult != null)
            {
                context.Request[OwinKeys.StormpathUserScheme] = RequestAuthenticationScheme.Bearer;
                return bearerAuthenticationResult;
            }

            var cookieAuthenticationResult = await TryCookieAuthenticationAsync(context);
            if (cookieAuthenticationResult != null)
            {
                context.Request[OwinKeys.StormpathUserScheme] = RequestAuthenticationScheme.Cookie;
                return cookieAuthenticationResult;
            }

            logger.LogTrace("No user found on request", nameof(GetUserAsync));
            return null;
        }

        private Task<dynamic> TryBearerAuthenticationAsync(IOwinEnvironment context)
        {
            var bearerHeaderParser = new BearerAuthenticationParser(context.Request.Headers.GetString("Authorization"),
                logger);
            if (!bearerHeaderParser.IsValid)
            {
                return Task.FromResult<dynamic>(null);
            }

            logger.LogInformation("Using Bearer header to authenticate request", nameof(TryBearerAuthenticationAsync));
            return ValidateAccessTokenAsync(context, bearerHeaderParser.Token);
        }

        private async Task<dynamic> TryCookieAuthenticationAsync(IOwinEnvironment context)
        {
            string[] rawCookies = null;

            if (!context.Request.Headers.TryGetValue("Cookie", out rawCookies))
            {
                logger.LogTrace("No cookie header found", nameof(TryCookieAuthenticationAsync));
                return null;
            }

            var cookieParser = new CookieParser(rawCookies, logger);

            if (cookieParser.Count == 0)
            {
                logger.LogTrace("No cookies parsed from header", nameof(TryCookieAuthenticationAsync));
                return null;
            }

            logger.LogTrace("Cookies found on request: " + string.Join(", ", cookieParser.AsEnumerable().Select(x => $"'{x.Key}'")), nameof(TryCookieAuthenticationAsync));

            var accessToken = cookieParser.Get(this.Configuration.Web.AccessTokenCookie.Name);
            var refreshToken = cookieParser.Get(this.Configuration.Web.RefreshTokenCookie.Name);

            // Attempt to validate incoming Access Token
            if (!string.IsNullOrEmpty(accessToken))
            {
                logger.LogTrace($"Found nonempty access token cookie '{this.Configuration.Web.AccessTokenCookie.Name}'", nameof(TryCookieAuthenticationAsync));

                var validAccount = await ValidateAccessTokenAsync(context, accessToken);
                if (validAccount != null)
                {
                    logger.LogInformation("Request authenticated using Access Token cookie", nameof(TryCookieAuthenticationAsync));
                    return validAccount;
                }
                else
                {
                    logger.LogInformation("Access token cookie was not valid", nameof(TryCookieAuthenticationAsync));
                }
            }

            // Try using refresh token instead
            if (!string.IsNullOrEmpty(refreshToken))
            {
                logger.LogTrace($"Found nonempty refresh token cookie '{this.Configuration.Web.RefreshTokenCookie.Name}'", nameof(TryCookieAuthenticationAsync));

                var refreshedAccount = await RefreshAccessTokenAsync(context, refreshToken);
                if (refreshedAccount != null)
                {
                    logger.LogInformation("Request authenticated using Refresh Token cookie", nameof(TryCookieAuthenticationAsync));
                    return refreshedAccount;
                }
                else
                {
                    logger.LogInformation("Refresh token cookie was not valid", nameof(TryCookieAuthenticationAsync));
                }
            }

            // Failed on both counts. Delete access and refresh token cookies if necessary
            if (cookieParser.Contains(this.Configuration.Web.AccessTokenCookie.Name))
            {
                Cookies.DeleteTokenCookie(context, this.Configuration.Web.AccessTokenCookie, logger);
            }
            if (cookieParser.Contains(this.Configuration.Web.RefreshTokenCookie.Name))
            {
                Cookies.DeleteTokenCookie(context, this.Configuration.Web.RefreshTokenCookie, logger);
            }

            logger.LogInformation("No access or refresh token cookies found", nameof(TryCookieAuthenticationAsync));
            return null;
        }

        private async Task<dynamic> ValidateAccessTokenAsync(IOwinEnvironment context, string accessTokenJwt)
        {
            var accessTokenValidator = new AccessTokenValidator(oktaClient, keyProvider, Configuration);

            var validationResult = await accessTokenValidator.ValidateAsync(accessTokenJwt, context.CancellationToken);
            if (!validationResult.Active)
            {
                logger.LogInformation("Failed to authenticate the request due to a malformed or expired access token.");
                return null;
            }

            dynamic account = null;
            try
            {
                account = await UserHelper.GetUserFromAccessTokenAsync(oktaClient, accessTokenJwt, logger, context.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(1000, ex, $"Failed to get account {validationResult.Uid}", nameof(ValidateAccessTokenAsync));
            }

            return account;
        }

        private async Task<dynamic> RefreshAccessTokenAsync(IOwinEnvironment context, string refreshTokenJwt)
        {
            GrantResult grantResult = null;
            try
            {
                grantResult = await oktaClient.PostRefreshGrantAsync(
                    Configuration.OktaEnvironment.AuthorizationServerId,
                    Configuration.OktaEnvironment.ClientId,
                    Configuration.OktaEnvironment.ClientSecret,
                    refreshTokenJwt,
                    context.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(1000, ex, $"Failed to refresh an access_token given refresh_token {refreshTokenJwt}.");
                return null;
            }

            // Get the account details
            dynamic account = null;
            try
            {
                account = await UserHelper.GetUserFromAccessTokenAsync(oktaClient, grantResult.AccessToken, logger, context.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(1000, ex, $"Failed to get account from new access_token");
                return null;
            }

            logger.LogTrace("Access token refreshed using Refresh token. Adding cookies to response");
            Cookies.AddTokenCookiesToResponse(context, grantResult, Configuration, logger);

            return account;
        }
    }
}
