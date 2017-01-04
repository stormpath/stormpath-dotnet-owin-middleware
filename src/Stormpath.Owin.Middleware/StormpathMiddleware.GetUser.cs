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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK;
using Stormpath.SDK.Account;
using Stormpath.SDK.Api;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Jwt;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Oauth;
using Stormpath.SDK.Shared.Extensions;

namespace Stormpath.Owin.Middleware
{
    public sealed partial class StormpathMiddleware
    {
        private async Task<IAccount> GetUserAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
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

            var apiAuthenticationResult = await TryBasicAuthenticationAsync(context, client, cancellationToken);
            if (apiAuthenticationResult != null)
            {
                context.Request[OwinKeys.StormpathUserScheme] = RequestAuthenticationScheme.ApiCredentials;
                return apiAuthenticationResult;
            }

            logger.Trace("No user found on request", nameof(GetUserAsync));
            return null;
        }

        private Task<IAccount> TryBasicAuthenticationAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            try
            {
                var basicHeaderParser = new BasicAuthenticationParser(context.Request.Headers.GetString("Authorization"), logger);
                if (!basicHeaderParser.IsValid)
                {
                    return Task.FromResult<IAccount>(null);
                }

                logger.Info("Using Basic header to authenticate request", nameof(TryBasicAuthenticationAsync));
                return ValidateApiCredentialsAsync(context, client, basicHeaderParser.Username, basicHeaderParser.Password, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Warn(ex, source: nameof(TryBasicAuthenticationAsync));
                return Task.FromResult<IAccount>(null);
            }
        }

        private Task<IAccount> TryBearerAuthenticationAsync(IOwinEnvironment context, IClient client)
        {
            var bearerHeaderParser = new BearerAuthenticationParser(context.Request.Headers.GetString("Authorization"),
                logger);
            if (!bearerHeaderParser.IsValid)
            {
                return Task.FromResult<IAccount>(null);
            }

            logger.Info("Using Bearer header to authenticate request", nameof(TryBearerAuthenticationAsync));
            return ValidateAccessTokenAsync(context, client, bearerHeaderParser.Token);
        }

        private async Task<IAccount> TryCookieAuthenticationAsync(IOwinEnvironment context, IClient client)
        {
            string[] rawCookies = null;

            if (!context.Request.Headers.TryGetValue("Cookie", out rawCookies))
            {
                logger.Trace("No cookie header found", nameof(TryCookieAuthenticationAsync));
                return null;
            }

            var cookieParser = new CookieParser(rawCookies, logger);

            if (cookieParser.Count == 0)
            {
                logger.Trace("No cookies parsed from header", nameof(TryCookieAuthenticationAsync));
                return null;
            }

            logger.Trace("Cookies found on request: " + cookieParser.AsEnumerable().Select(x => $"'{x.Key}'").Join(", "), nameof(TryCookieAuthenticationAsync));

            var accessToken = cookieParser.Get(this.Configuration.Web.AccessTokenCookie.Name);
            var refreshToken = cookieParser.Get(this.Configuration.Web.RefreshTokenCookie.Name);

            // Attempt to validate incoming Access Token
            if (!string.IsNullOrEmpty(accessToken))
            {
                logger.Trace($"Found nonempty access token cookie '{this.Configuration.Web.AccessTokenCookie.Name}'", nameof(TryCookieAuthenticationAsync));

                var validAccount = await ValidateAccessTokenAsync(context, client, accessToken);
                if (validAccount != null)
                {
                    logger.Info("Request authenticated using Access Token cookie", nameof(TryCookieAuthenticationAsync));
                    return validAccount;
                }
                else
                {
                    logger.Info("Access token cookie was not valid", nameof(TryCookieAuthenticationAsync));
                }
            }

            // Try using refresh token instead
            if (!string.IsNullOrEmpty(refreshToken))
            {
                logger.Trace($"Found nonempty refresh token cookie '{this.Configuration.Web.RefreshTokenCookie.Name}'", nameof(TryCookieAuthenticationAsync));

                var refreshedAccount = await RefreshAccessTokenAsync(context, client, refreshToken);
                if (refreshedAccount != null)
                {
                    logger.Info("Request authenticated using Refresh Token cookie", nameof(TryCookieAuthenticationAsync));
                    return refreshedAccount;
                }
                else
                {
                    logger.Info("Refresh token cookie was not valid", nameof(TryCookieAuthenticationAsync));
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

            logger.Info("No access or refresh token cookies found", nameof(TryCookieAuthenticationAsync));
            return null;
        }

        private async Task<IAccount> ValidateAccessTokenAsync(IOwinEnvironment context, IClient client, string accessTokenJwt)
        {
            var request = OauthRequests.NewJwtAuthenticationRequest()
                .SetJwt(accessTokenJwt)
                .Build();

            var application = await client.GetApplicationAsync(this.Configuration.Application.Href, context.CancellationToken);
            var authenticator = application.NewJwtAuthenticator();
            if (this.Configuration.Web.Oauth2.Password.ValidationStrategy == WebOauth2TokenValidationStrategy.Local)
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

            var application = await client.GetApplicationAsync(this.Configuration.Application.Href, context.CancellationToken);
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
            Cookies.AddTokenCookiesToResponse(context, client, grantResult, this.Configuration, logger);

            return account;
        }

        private async Task<IAccount> ValidateApiCredentialsAsync(
            IOwinEnvironment context,
            IClient client,
            string id,
            string secret,
            CancellationToken cancellationToken)
        {
            var application = await client
                .GetApplicationAsync(Configuration.Application.Href, cancellationToken)
                .ConfigureAwait(false);

            var apiKey = await application
                .GetApiKeys()
                .Where(x => x.Id == id)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (apiKey == null)
            {
                logger.Info($"API key with ID {id} was not found", nameof(ValidateApiCredentialsAsync));
                return null;
            }

            if (apiKey.Status != ApiKeyStatus.Enabled)
            {
                logger.Info($"API key with ID {id} was found, but was disabled", nameof(ValidateApiCredentialsAsync));
                return null;
            }

            if (!apiKey.Secret.Equals(secret, StringComparison.Ordinal))
            {
                logger.Info($"API key with ID {id} was found, but secret did not match", nameof(ValidateApiCredentialsAsync));
                return null;
            }

            var account = await apiKey.GetAccountAsync(cancellationToken).ConfigureAwait(false);

            if (account.Status != AccountStatus.Enabled)
            {
                logger.Info($"API key with ID {id} was found, but the account is not enabled", nameof(ValidateApiCredentialsAsync));
                return null;
            }

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
