// <copyright file="LogoutRoute.cs" company="Stormpath, Inc.">
// Copyright (c) 2016 Stormpath, Inc.
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

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Jwt;

namespace Stormpath.Owin.Middleware.Route
{
    public class LogoutRoute : AbstractRoute
    {
        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            await HandleLogout(context, client, cancellationToken);

            await HttpResponse.Redirect(context, _configuration.Web.Logout.NextUri);
            return true;
        }

        protected override async Task<bool> PostJsonAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            await HandleLogout(context, client, cancellationToken);

            await JsonResponse.Ok(context);
            return true;
        }

        private async Task HandleLogout(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // Remove user from request
            context.Request[OwinKeys.StormpathUser] = null;

            string[] rawCookies;
            context.Request.Headers.TryGetValue("Cookie", out rawCookies);
            var cookieParser = new CookieParser(rawCookies, _logger);

            await RevokeTokens(context, client, cookieParser, cancellationToken);
            
            DeleteCookies(context, cookieParser);
        }

        private async Task RevokeTokens(IOwinEnvironment context, IClient client, CookieParser cookieParser, CancellationToken cancellationToken)
        {
            var accessToken = cookieParser.Get(_configuration.Web.AccessTokenCookie.Name);
            var refreshToken = cookieParser.Get(_configuration.Web.RefreshTokenCookie.Name);

            var deleteAccessTokenTask = Task.FromResult(false);
            var deleteRefreshTokenTask = Task.FromResult(false);

            string jti = string.Empty;
            if (IsValidJwt(accessToken, client, out jti))
            {
                var accessTokenResource = await client.GetAccessTokenAsync($"/accessTokens/{jti}", cancellationToken);
                deleteAccessTokenTask = accessTokenResource.DeleteAsync(cancellationToken);
            }

            if (IsValidJwt(refreshToken, client, out jti))
            {
                var refreshTokenResource = await client.GetRefreshTokenAsync($"/refreshTokens/{jti}", cancellationToken);
                deleteRefreshTokenTask = refreshTokenResource.DeleteAsync(cancellationToken);
            }

            await Task.WhenAll(deleteAccessTokenTask, deleteRefreshTokenTask);
        }

        private bool IsValidJwt(string jwt, IClient client, out string jti)
        {
            jti = null;

            if (string.IsNullOrEmpty(jwt))
            {
                return false;
            }

            try
            {
                var parsed = client.NewJwtParser()
                    .SetSigningKey(client.Configuration.Client.ApiKey.Secret, Encoding.UTF8)
                    .Parse(jwt);
                jti = parsed.Body.Id;
                return true;
            }
            catch (InvalidJwtException)
            {
                return false;
            }
        }

        private void DeleteCookies(IOwinEnvironment context, CookieParser cookieParser)
        {
            if (cookieParser.Contains(_configuration.Web.AccessTokenCookie.Name))
            {
                Cookies.Delete(context, _configuration.Web.AccessTokenCookie, _logger);
            }
            if (cookieParser.Contains(_configuration.Web.RefreshTokenCookie.Name))
            {
                Cookies.Delete(context, _configuration.Web.RefreshTokenCookie, _logger);
            }
        }
    }
}
