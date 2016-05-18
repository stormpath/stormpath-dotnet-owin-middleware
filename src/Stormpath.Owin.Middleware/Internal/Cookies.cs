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
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Oauth;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class Cookies
    {
        public static DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static string DateFormat = "ddd, dd-MMM-yyyy HH:mm:ss"; // + GMT

        public static string FormatDate(DateTimeOffset dateTimeOffset)
            => $"{dateTimeOffset.UtcDateTime.ToString(DateFormat, CultureInfo.InvariantCulture)} GMT";

        public static void AddCookiesToResponse(IOwinEnvironment context, IClient client, IOauthGrantAuthenticationResult grantResult, StormpathConfiguration configuration, ILogger logger)
        {
            bool isSecureRequest = context.Request.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(grantResult.AccessTokenString))
            {
                var expirationDate = client.NewJwtParser().Parse(grantResult.AccessTokenString).Body.Expiration;
                SetTokenCookie(context, configuration.Web.AccessTokenCookie, grantResult.AccessTokenString, expirationDate, isSecureRequest, logger);
            }

            if (!string.IsNullOrEmpty(grantResult.RefreshTokenString))
            {
                var expirationDate = client.NewJwtParser().Parse(grantResult.RefreshTokenString).Body.Expiration;
                SetTokenCookie(context, configuration.Web.RefreshTokenCookie, grantResult.RefreshTokenString, expirationDate, isSecureRequest, logger);
            }
        }

        public static void DeleteTokenCookies(IOwinEnvironment context, WebConfiguration webConfiguration, ILogger logger)
        {
            Delete(context, webConfiguration.AccessTokenCookie, logger);
            Delete(context, webConfiguration.RefreshTokenCookie, logger);
        }

        public static void Delete(IOwinEnvironment context, WebCookieConfiguration cookieConfiguration, ILogger logger)
            => SetTokenCookie(context, cookieConfiguration, string.Empty, Epoch, false, logger);

        private static void SetTokenCookie(
            IOwinEnvironment context,
            WebCookieConfiguration cookieConfig,
            string value,
            DateTimeOffset? expiration,
            bool isSecureRequest,
            ILogger logger)
        {
            var keyValuePair = $"{Uri.EscapeDataString(cookieConfig.Name)}={Uri.EscapeDataString(value)}";
            var domain = $"domain={cookieConfig.Domain}";

            var pathToken = string.IsNullOrEmpty(cookieConfig.Path)
                ? "/"
                : cookieConfig.Path;
            var path = $"path={pathToken}";

            
            var expires = expiration != null
                ? $"expires={FormatDate(expiration.Value)}"
                : null;

            var httpOnly = cookieConfig.HttpOnly
                ? "HttpOnly"
                : null;

            var includeSecureToken = cookieConfig.Secure == null
                ? isSecureRequest
                : cookieConfig.Secure.Value;
            var secure = includeSecureToken
                ? "secure"
                : null;

            var setCookieValue = string.Join("; ", new string[] { keyValuePair, domain, path, expires, httpOnly, secure });

            logger.Trace($"Adding cookie to request: '{setCookieValue}'", nameof(SetTokenCookie));

            context.Response.OnSendingHeaders(_ =>
            {
                context.Response.Headers.AddString("Set-Cookie", setCookieValue);
            }, null);
        }
    }
}
