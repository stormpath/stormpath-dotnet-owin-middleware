// <copyright file="Cookie.cs" company="Stormpath, Inc.">
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

using System;
using System.Globalization;
using Stormpath.Configuration.Abstractions;
using Stormpath.Configuration.Abstractions.Model;
using Stormpath.Owin.Common;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.SDK.Client;
using Stormpath.SDK.Oauth;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class Cookies
    {
        public static string DateFormat = "ddd, dd-MMM-yyyy HH:mm:ss"; // + GMT

        public static string FormatDate(DateTimeOffset dateTimeOffset)
            => $"{dateTimeOffset.UtcDateTime.ToString(DateFormat, CultureInfo.InvariantCulture)} GMT";

        public static void AddToResponse(IOwinEnvironment context, IClient client, IOauthGrantAuthenticationResult grantResult, StormpathConfiguration configuration)
        {
            bool isSecureRequest = context.Request.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(grantResult.AccessTokenString))
            {
                SetTokenCookie(context, grantResult.AccessTokenString, client, configuration.Web.AccessTokenCookie, isSecureRequest);
            }

            if (!string.IsNullOrEmpty(grantResult.RefreshTokenString))
            {
                SetTokenCookie(context, grantResult.RefreshTokenString, client, configuration.Web.RefreshTokenCookie, isSecureRequest);
            }
        }

        private static void SetTokenCookie(IOwinEnvironment context, string token, IClient client, WebCookieConfiguration cookieConfig, bool isSecureRequest)
        {
            var keyValuePair = $"{Uri.EscapeDataString(cookieConfig.Name)}={Uri.EscapeDataString(token)}";
            var domain = $"domain={cookieConfig.Domain}";

            var pathToken = string.IsNullOrEmpty(cookieConfig.Path)
                ? "/"
                : cookieConfig.Path;
            var path = $"path={(pathToken)}";

            var expirationDate = client.NewJwtParser().Parse(token).Body.Expiration;
            var expires = expirationDate != null
                ? $"expires={FormatDate(expirationDate.Value)}"
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

        public static void DeleteTokenCookies(IOwinEnvironment context, WebConfiguration webConfiguration)
        {
            var deleteAccessToken = string.Concat(
                webConfiguration.AccessTokenCookie.Name,
                "=",
                "; path=",
                string.IsNullOrEmpty(webConfiguration.AccessTokenCookie.Path) ? "/" : webConfiguration.AccessTokenCookie.Path,
                "; expires=",
                FormatDate(new DateTimeOffset(1970, 01, 01, 00, 00, 00, TimeSpan.Zero)));

            var deleteRefreshToken = string.Concat(
                webConfiguration.RefreshTokenCookie.Name,
                "=",
                "; path=",
                string.IsNullOrEmpty(webConfiguration.RefreshTokenCookie.Path) ? "/" : webConfiguration.RefreshTokenCookie.Path,
                "; expires=",
                FormatDate(new DateTimeOffset(1970, 01, 01, 00, 00, 00, TimeSpan.Zero)));

            context.Response.Headers.AddString("Set-Cookie", deleteAccessToken);
            context.Response.Headers.AddString("Set-Cookie", deleteRefreshToken);
        }
    }
}
