// <copyright file="StormpathMiddleware.GetUser.cs" company="Stormpath, Inc.">
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
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    public sealed partial class StormpathMiddleware
    {
        private async Task<ICompatibleOktaAccount> GetUserAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            // TODO: Reuse the same client across requests
            var oktaClient = new OktaClient(Configuration.Org, Configuration.ApiToken, userAgentBuilder, logger);

            var bearerAuthenticationResult = await TryBearerAuthenticationAsync(context, oktaClient);
            if (bearerAuthenticationResult != null)
            {
                context.Request[OwinKeys.StormpathUserScheme] = RequestAuthenticationScheme.Bearer;
                return bearerAuthenticationResult;
            }

            var cookieAuthenticationResult = await TryCookieAuthenticationAsync(context, oktaClient);
            if (cookieAuthenticationResult != null)
            {
                context.Request[OwinKeys.StormpathUserScheme] = RequestAuthenticationScheme.Cookie;
                return cookieAuthenticationResult;
            }

            var apiAuthenticationResult = await TryBasicAuthenticationAsync(context, oktaClient);
            if (apiAuthenticationResult != null)
            {
                context.Request[OwinKeys.StormpathUserScheme] = RequestAuthenticationScheme.ApiCredentials;
                return apiAuthenticationResult;
            }

            logger.LogTrace("No user found on request", nameof(GetUserAsync));
            return null;
        }

        private Task<ICompatibleOktaAccount> TryBasicAuthenticationAsync(IOwinEnvironment context, IOktaClient client)
        {
            var basicHeaderParser = new BasicAuthenticationParser(
                context.Request.Headers.GetString("Authorization"),
                logger);
            if (!basicHeaderParser.IsValid)
            {
                return Task.FromResult<ICompatibleOktaAccount>(null);
            }

            try
            {
                logger.LogInformation("Using Basic header to authenticate request");
                return ValidateApiCredentialsAsync(context, client, basicHeaderParser.Username, basicHeaderParser.Password);
            }
            catch (Exception ex)
            {
                logger.LogWarning(1001, ex, "Error during TryBasicAuthenticationAsync");
                return Task.FromResult<ICompatibleOktaAccount>(null);
            }
        }

        private Task<ICompatibleOktaAccount> TryBearerAuthenticationAsync(IOwinEnvironment context, IOktaClient oktaClient)
        {
            var bearerHeaderParser = new BearerAuthenticationParser(
                context.Request.Headers.GetString("Authorization"),
                logger);
            if (!bearerHeaderParser.IsValid)
            {
                return Task.FromResult<ICompatibleOktaAccount>(null);
            }

            logger.LogInformation("Using Bearer header to authenticate request", nameof(TryBearerAuthenticationAsync));
            return ValidateAccessTokenAsync(context, oktaClient, bearerHeaderParser.Token);
        }

        private async Task<ICompatibleOktaAccount> TryCookieAuthenticationAsync(IOwinEnvironment context, IOktaClient oktaClient)
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

                var validAccount = await ValidateAccessTokenAsync(context, oktaClient, accessToken);
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

                var refreshedAccount = await RefreshAccessTokenAsync(context, oktaClient, refreshToken);
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

        private async Task<ICompatibleOktaAccount> ValidateAccessTokenAsync(IOwinEnvironment context, IOktaClient oktaClient, string accessTokenJwt)
        {
            var accessTokenValidator = new AccessTokenValidator(oktaClient, keyProvider, Configuration);

            var validationResult = await accessTokenValidator.ValidateAsync(accessTokenJwt, context.CancellationToken);
            if (!validationResult.Active)
            {
                logger.LogInformation("Failed to authenticate the request due to a malformed or expired access token.");
                return null;
            }

            ICompatibleOktaAccount account = null;
            try
            {
                account = await UserHelper.GetAccountFromAccessTokenAsync(oktaClient, accessTokenJwt, logger, context.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(1000, ex, $"Failed to get account {validationResult.Uid}", nameof(ValidateAccessTokenAsync));
            }

            return account;
        }

        private async Task<ICompatibleOktaAccount> RefreshAccessTokenAsync(IOwinEnvironment context, IOktaClient oktaClient, string refreshTokenJwt)
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
            ICompatibleOktaAccount account = null;
            try
            {
                account = await UserHelper.GetAccountFromAccessTokenAsync(oktaClient, grantResult.AccessToken, logger, context.CancellationToken);
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

        private async Task<ICompatibleOktaAccount> ValidateApiCredentialsAsync(
            IOwinEnvironment context,
            IOktaClient client,
            string id,
            string secret)
        {
            var apiKeyResolver = new ApiKeyResolver(client);
            var apiKey = await apiKeyResolver.LookupApiKeyIdAsync(id, secret, context.CancellationToken);

            if (apiKey == null)
            {
                logger.LogInformation($"API key with ID {id} and matching secret was not found", nameof(ValidateApiCredentialsAsync));
                return null;
            }

            return new CompatibleOktaAccount(apiKey.User);
        }
    }
}
