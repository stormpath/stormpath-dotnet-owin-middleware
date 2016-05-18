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
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Jwt;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Oauth;

namespace Stormpath.Owin.Middleware
{
    public sealed partial class StormpathMiddleware
    {
        private async Task<IAccount> GetUserAsync(IOwinEnvironment context, IClient client)
        {
            var bearerAuthenticationResult = await TryBearerAuthenticationAsync(context, client);
            if (bearerAuthenticationResult != null)
            {
                context.Request[OwinKeys.StormpathUserScheme] = RequestAuthenticationScheme.Bearer;
                return bearerAuthenticationResult;
            }

            var cookieAuthenticationResult = await TryCookieAuthenticationAsync(context, client);
            if (cookieAuthenticationResult != null)
            {
                context.Request[OwinKeys.StormpathUserScheme] = RequestAuthenticationScheme.Cookie;
                return cookieAuthenticationResult;
            }

            var apiAuthenticationResult = await TryBasicAuthenticationAsync(context, client);
            if (apiAuthenticationResult != null)
            {
                context.Request[OwinKeys.StormpathUserScheme] = RequestAuthenticationScheme.ApiCredentials;
                return apiAuthenticationResult;
            }

            logger.Trace("No user found on request", nameof(GetUserAsync));
            return null;
        }

        private Task<IAccount> TryBasicAuthenticationAsync(IOwinEnvironment context, IClient client)
        {
            this.logger.Warn("Basic Authentication is not yet supported", nameof(TryBasicAuthenticationAsync));

            // TODO Basic auth
            return Task.FromResult<IAccount>(null);
        }

        private Task<IAccount> TryBearerAuthenticationAsync(IOwinEnvironment context, IClient client)
        {
            var bearerHeader = context.Request.Headers.GetString("Authorization");
            bool isValid = !string.IsNullOrEmpty(bearerHeader) && bearerHeader.StartsWith("Bearer ", StringComparison.Ordinal);
            if (!isValid)
            {
                logger.Trace("No Bearer header found", nameof(TryBearerAuthenticationAsync));
                return Task.FromResult<IAccount>(null);
            }

            var bearerPayload = bearerHeader?.Substring(7); // Bearer[ ]
            if (string.IsNullOrEmpty(bearerPayload))
            {
                logger.Warn("Found Bearer header, but payload was empty", nameof(TryBearerAuthenticationAsync));
                return Task.FromResult<IAccount>(null);
            }

            logger.Info("Request authenticated using Bearer authentication", nameof(TryBearerAuthenticationAsync));
            return ValidateAccessTokenAsync(context, client, bearerPayload);
        }

        private async Task<IAccount> TryCookieAuthenticationAsync(IOwinEnvironment context, IClient client)
        {
            var cookieHeader = context.Request.Headers.GetString("Cookie");
            if (string.IsNullOrEmpty(cookieHeader))
            {
                logger.Trace("No authentication cookie found", nameof(TryCookieAuthenticationAsync));
                return null;
            }

            var cookieParser = new CookieParser(cookieHeader, logger);
            var accessToken = cookieParser.Get(this.configuration.Web.AccessTokenCookie.Name);
            var refreshToken = cookieParser.Get(this.configuration.Web.RefreshTokenCookie.Name);

            // Attempt to validate incoming Access Token
            if (!string.IsNullOrEmpty(accessToken))
            {
                var validAccount = await ValidateAccessTokenAsync(context, client, accessToken);
                if (validAccount != null)
                {
                    logger.Info("Request authenticated using Access Token cookie", nameof(TryCookieAuthenticationAsync));
                    return validAccount;
                }
            }

            // Try using refresh token instead
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshedAccount = await RefreshAccessTokenAsync(context, client, refreshToken);
                if (refreshedAccount != null)
                {
                    logger.Info("Request authenticated using Refresh Token cookie", nameof(TryCookieAuthenticationAsync));
                    return refreshedAccount;
                }
            }

            // Failed on both counts. Delete access and refresh token cookies
            Cookies.DeleteTokenCookies(context, this.configuration.Web, logger);
            logger.Info("Request contained invalid cookies, not authenticated", nameof(TryCookieAuthenticationAsync));
            return null;
        }

        private async Task<IAccount> ValidateAccessTokenAsync(IOwinEnvironment context, IClient client, string accessTokenJwt)
        {
            var request = OauthRequests.NewJwtAuthenticationRequest()
                .SetJwt(accessTokenJwt)
                .Build();

            var application = await client.GetApplicationAsync(this.configuration.Application.Href);
            var authenticator = application.NewJwtAuthenticator();
            if (this.configuration.Web.Oauth2.Password.ValidationStrategy == WebOauth2TokenValidationStrategy.Local)
            {
                authenticator.WithLocalValidation();
            }

            IAccessToken result = null;
            try
            {
                result = await authenticator.AuthenticateAsync(request, context.CancellationToken);

            }
            catch (InvalidJwtException jwex)
            {
                logger.Info($"Failed to authenticate the request due to a malformed or expired access token. Message: '{jwex.Message}'", nameof(ValidateAccessTokenAsync));
                return null;
            }
            catch (ResourceException rex)
            {
                logger.Warn(rex, "Failed to authenticate the request. Invalid access_token found.", nameof(ValidateAccessTokenAsync));
                return null;
            }

            IAccount account = null;
            try
            {
                account = await GetExpandedAccountAsync(client, result, context.CancellationToken);
            }
            catch (ResourceException ex)
            {
                logger.Error(ex, $"Failed to get account {result.AccountHref}", nameof(ValidateAccessTokenAsync));
            }

            return account;
        }

        private async Task<IAccount> RefreshAccessTokenAsync(IOwinEnvironment context, IClient client, string refreshTokenJwt)
        {
            // Attempt refresh grant against Stormpath
            var request = OauthRequests.NewRefreshGrantRequest()
                .SetRefreshToken(refreshTokenJwt)
                .Build();

            var application = await client.GetApplicationAsync(this.configuration.Application.Href);
            var authenticator = application.NewRefreshGrantAuthenticator();

            IOauthGrantAuthenticationResult grantResult = null;
            try
            {
                grantResult = await authenticator.AuthenticateAsync(request, context.CancellationToken);
            }
            catch (InvalidJwtException jwex)
            {
                logger.Info($"Failed to authenticate the request due to a malformed or expired refresh token. Message: '{jwex.Message}'", nameof(RefreshAccessTokenAsync));
                return null;
            }
            catch (ResourceException rex)
            {
                logger.Warn(rex, "Failed to refresh an access_token given a refresh_token.");
                return null;
            }

            // Get a new access token
            IAccessToken newAccessToken = null;
            try
            {
                newAccessToken = await grantResult.GetAccessTokenAsync(context.CancellationToken);
            }
            catch (ResourceException rex)
            {
                logger.Error(rex, "Failed to get a new access token after receiving grant response.", nameof(RefreshAccessTokenAsync));
            }

            // Get the account details
            IAccount account = null;
            try
            {
                account = await GetExpandedAccountAsync(client, newAccessToken, context.CancellationToken);
            }
            catch (ResourceException rex)
            {
                logger.Error(rex, $"Failed to get account {newAccessToken.AccountHref}", nameof(RefreshAccessTokenAsync));
                return null;
            }

            logger.Trace("Access token refreshed using Refresh token. Adding cookies to response", nameof(RefreshAccessTokenAsync));
            Cookies.AddCookiesToResponse(context, client, grantResult, this.configuration, logger);

            return account;
        }

        private Task<IAccount> GetExpandedAccountAsync(IClient client, IAccessToken accessToken, CancellationToken cancellationToken)
        {
            // TODO: This is a bit of a hack until we have better support for scoped user agents through the stack.
            return client.GetAccountAsync(
                accessToken.AccountHref,
                cancellationToken);
        }
    }
}
