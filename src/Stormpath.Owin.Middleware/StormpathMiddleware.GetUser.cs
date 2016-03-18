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
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions.Model;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Oauth;

namespace Stormpath.Owin.Middleware
{
    public sealed partial class StormpathMiddleware
    {
        public async Task GetUserAsync(IOwinEnvironment context, IClient client)
        {
            // TODO API authentication

            var cookieHeader = context.Request.Headers.GetString("Cookie");
            if (string.IsNullOrEmpty(cookieHeader))
            {
                return;
            }

            var cookieParser = new CookieParser(cookieHeader);
            var accessToken = cookieParser.Get(this.configuration.Web.AccessTokenCookie.Name);
            var refreshToken = cookieParser.Get(this.configuration.Web.RefreshTokenCookie.Name);

            // Attempt to validate incoming Access Token
            if (!string.IsNullOrEmpty(accessToken))
            {
                var passwordGrantSuccess = await AttemptPasswordGrantAsync(context, client, accessToken);
                if (passwordGrantSuccess)
                {
                    return;
                }
            }

            // Try using refresh token instead
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshGrantSuccess = await AttemptRefreshGrantAsync(context, client, refreshToken);
                if (refreshGrantSuccess)
                {
                    return;
                }
            }

            // Failed on both counts. Delete access and refresh token cookies in response
            DeleteTokenCookies(context);
        }

        private async Task<bool> AttemptPasswordGrantAsync(IOwinEnvironment context, IClient client, string accessTokenJwt)
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
            catch (ResourceException rex)
            {
                logger.Info($"Failed to authenticate the request. Invalid access_token found. Message: '{rex.DeveloperMessage}'", "GetUserAsync");
                return false;
            }

            IAccount account = null;
            try
            {
                account = await GetExpandedAccountAsync(result, context.CancellationToken);
            }
            catch (ResourceException)
            {
                logger.Info($"Failed to get account {account.Href}", "GetUserAsync"); // TODO result.AccountHref
                return false;
            }

            AddUserToRequest(context, account);

            return true;
        }

        private async Task<bool> AttemptRefreshGrantAsync(IOwinEnvironment context, IClient client, string refreshTokenJwt)
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
            catch (ResourceException rex)
            {
                logger.Info($"Failed to refresh an access_token given a refresh_token. Message: '{rex.DeveloperMessage}'");
                return false;
            }

            // Get a new access token
            IAccessToken newAccessToken = null;
            try
            {
                newAccessToken = await grantResult.GetAccessTokenAsync(context.CancellationToken);
            }
            catch (ResourceException rex)
            {
                logger.Info($"Failed to get a new access token after receiving grant response. Message: '{rex.DeveloperMessage}'");
            }

            // Get the account details
            IAccount account = null;
            try
            {
                account = await GetExpandedAccountAsync(newAccessToken, context.CancellationToken);
            }
            catch (ResourceException)
            {
                logger.Info($"Failed to get account {account.Href}", "AttemptRefreshGrantAsync"); // TODO result.AccountHref
                return false;
            }

            AddUserToRequest(context, account);

            AddCookiesToResponse(context, client, grantResult);

            return true;
        }

        private Task<IAccount> GetExpandedAccountAsync(IAccessToken accessToken, CancellationToken cancellationToken)
        {
            return accessToken.GetAccountAsync(cancellationToken); // TODO expand
        }

        private void AddUserToRequest(IOwinEnvironment context, IAccount account)
        {
            context.Request[OwinKeys.StormpathUser] = account;

            // Build an IPrincipal and return it
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Email, account.Email));
            claims.Add(new Claim(ClaimTypes.GivenName, account.GivenName));
            claims.Add(new Claim(ClaimTypes.Surname, account.Surname));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, account.Username));
            var identity = new ClaimsIdentity(claims, "Token");
            var principal = new ClaimsPrincipal(identity);
            context.Request[OwinKeys.RequestUser] = principal;
            context.Request[OwinKeys.RequestUserLegacy] = principal;

            // TODO deal with groups/scopes
        }

        private void AddCookiesToResponse(IOwinEnvironment context, IClient client, IOauthGrantAuthenticationResult grantResult)
        {
            bool isSecureRequest = context.Request.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(grantResult.AccessTokenString))
            {
                SetTokenCookie(context, grantResult.AccessTokenString, client, this.configuration.Web.AccessTokenCookie, isSecureRequest);
            }

            if (!string.IsNullOrEmpty(grantResult.RefreshTokenString))
            {
                SetTokenCookie(context, grantResult.RefreshTokenString, client, this.configuration.Web.RefreshTokenCookie, isSecureRequest);
            }
        }

        private void SetTokenCookie(IOwinEnvironment context, string token, IClient client, WebCookieConfiguration cookieConfig, bool isSecureRequest)
        {
            var keyValuePair = $"{Uri.EscapeDataString(cookieConfig.Name)}={Uri.EscapeDataString(token)}";
            var domain = $"domain={cookieConfig.Domain}";

            var pathToken = string.IsNullOrEmpty(cookieConfig.Path)
                ? "/"
                : cookieConfig.Path;
            var path = $"path={(pathToken)}";

            var expirationDate = client.NewJwtParser().Parse(token).Body.Expiration;
            var expires = expirationDate != null
                ? $"expires={Cookie.FormatDate(expirationDate.Value)}"
                : null;

            var httpOnly = cookieConfig.HttpOnly ?? false
                ? "HttpOnly"
                : null;

            var includeSecureToken = cookieConfig.Secure == null
                ? isSecureRequest
                : cookieConfig.Secure.Value;
            var secure = includeSecureToken
                ? "secure"
                : null;

            var setCookieValue = string.Join("; ", new string[] { keyValuePair, domain, path, expires, httpOnly, secure });

            context.Response.Headers.AddString("Set-Cookie", setCookieValue);
        }

        private void DeleteTokenCookies(IOwinEnvironment context)
        {
            var deleteAccessToken = string.Concat(
                this.configuration.Web.AccessTokenCookie.Name,
                "=",
                "; path=",
                string.IsNullOrEmpty(this.configuration.Web.AccessTokenCookie.Path) ? "/" : this.configuration.Web.AccessTokenCookie.Path,
                "; expires=",
                Cookie.FormatDate(new DateTimeOffset(1970, 01, 01, 00, 00, 00, TimeSpan.Zero)));

            var deleteRefreshToken = string.Concat(
                this.configuration.Web.RefreshTokenCookie.Name,
                "=",
                "; path=",
                string.IsNullOrEmpty(this.configuration.Web.RefreshTokenCookie.Path) ? "/" : this.configuration.Web.RefreshTokenCookie.Path,
                "; expires=",
                Cookie.FormatDate(new DateTimeOffset(1970, 01, 01, 00, 00, 00, TimeSpan.Zero)));

            context.Response.Headers.AddString("Set-Cookie", deleteAccessToken);
            context.Response.Headers.AddString("Set-Cookie", deleteRefreshToken);
        }
    }
}
