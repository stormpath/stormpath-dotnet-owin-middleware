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
using System.Linq;
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

        public static void AddTokenCookiesToResponse(IOwinEnvironment context, IClient client, IOauthGrantAuthenticationResult grantResult, StormpathConfiguration configuration, ILogger logger)
        {
            if (!string.IsNullOrEmpty(grantResult.AccessTokenString))
            {
                var expirationDate = client.NewJwtParser().Parse(grantResult.AccessTokenString).Body.Expiration;
                SetTokenCookie(context, configuration.Web.AccessTokenCookie, grantResult.AccessTokenString, expirationDate, IsSecureRequest(context), logger);
            }

            if (!string.IsNullOrEmpty(grantResult.RefreshTokenString))
            {
                var expirationDate = client.NewJwtParser().Parse(grantResult.RefreshTokenString).Body.Expiration;
                SetTokenCookie(context, configuration.Web.RefreshTokenCookie, grantResult.RefreshTokenString, expirationDate, IsSecureRequest(context), logger);
            }
        }

        public static void DeleteTokenCookie(IOwinEnvironment context, WebCookieConfiguration cookieConfiguration, ILogger logger)
        {
            logger.Trace($"Deleting cookie '{cookieConfiguration.Name}' on response");

            SetTokenCookie(context, cookieConfiguration, string.Empty, Epoch, IsSecureRequest(context), logger);
        }

        private static bool IsSecureRequest(IOwinEnvironment context)
            => context.Request.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

        private static void SetTokenCookie(
            IOwinEnvironment context,
            WebCookieConfiguration cookieConfig,
            string value,
            DateTimeOffset? expiration,
            bool isSecureRequest,
            ILogger logger)
        {
            var domain = !string.IsNullOrEmpty(cookieConfig.Domain)
                ? $"domain={cookieConfig.Domain}"
                : null;

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

            var includeSecureToken = cookieConfig.Secure ?? isSecureRequest;
            var secure = includeSecureToken
                ? "Secure"
                : null;

            var attributeTokens = 
                new[] { domain, path, expires, httpOnly, secure }
                .Where(t => !string.IsNullOrEmpty(t));

            var attributes = string.Join("; ", attributeTokens);

            SetCookie(context, cookieConfig.Name, value, attributes, logger);
        }

        private static void SetCookie(
            IOwinEnvironment context,
            string name,
            string value,
            string attributes,
            ILogger logger)
        {
            var cookieFormat = $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";

            if (!string.IsNullOrEmpty(attributes))
            {
                cookieFormat += $"; {attributes}";
            }

            logger.Trace($"Adding cookie to response: '{cookieFormat}'", nameof(SetCookie));

            context.Response.OnSendingHeaders(_ =>
            {
                context.Response.Headers.AddString("Set-Cookie", cookieFormat);
            }, null);
        }
    }
}
