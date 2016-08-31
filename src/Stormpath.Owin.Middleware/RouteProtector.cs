// <copyright file="RouteProtector.cs" company="Stormpath, Inc.">
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
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Account;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware
{
    /// <summary>
    /// Represents the logic that protects routes and responds to unauthorized access.
    /// </summary>
    public sealed class RouteProtector
    {
        public static string AnyScheme = "*";

        private readonly ApplicationConfiguration appConfiguration;
        private readonly WebConfiguration webConfiguration;
        private readonly ILogger logger;

        private readonly Action<WebCookieConfiguration> deleteCookie;
        private readonly Action<int> setStatusCode;
        private readonly Action<string, string> setHeader;
        private readonly Action<string> redirect;

        /// <summary>
        /// Creates a new instance of the <see cref="RouteProtector"/> class.
        /// </summary>
        /// <param name="appConfiguration">The active Stormpath <see cref="ApplicationConfiguration">application configuration</see>.</param>
        /// <param name="webConfiguration">The active Stormpath <see cref="WebConfiguration">web configuration</see>.</param>
        /// <param name="deleteCookieAction">Delegate to delete cookies in the response.</param>
        /// <param name="setStatusCodeAction">Delegate to set the response status code.</param>
        /// <param name="setHeaderAction">Delegate to set a header in the response.</param>
        /// <param name="redirectAction">Delegate to set the response Location header.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public RouteProtector(
            ApplicationConfiguration appConfiguration,
            WebConfiguration webConfiguration,
            Action<WebCookieConfiguration> deleteCookieAction,
            Action<int> setStatusCodeAction,
            Action<string, string> setHeaderAction,
            Action<string> redirectAction,
            ILogger logger)
        {
            this.appConfiguration = appConfiguration;
            this.webConfiguration = webConfiguration;
            this.logger = logger;

            deleteCookie = deleteCookieAction;
            setStatusCode = setStatusCodeAction;
            setHeader = setHeaderAction;
            redirect = redirectAction;
        }

        /// <summary>
        /// Checks whether a properly-authenticated user is making this request.
        /// </summary>
        /// <remarks><paramref name="authenticationScheme"/> and <paramref name="account"/> are available in the OWIN environment as <see cref="Abstractions.OwinKeys.StormpathUser"/> and <see cref="Abstractions.OwinKeys.StormpathUserScheme"/>, respectively.</remarks>
        /// <param name="authenticationScheme">The authentication scheme used for the request.</param>
        /// <param name="requiredAuthenticationScheme">The authentication scheme that must be used for this route, or <see cref="AnyScheme"/>.</param>
        /// <param name="account">The Stormpath Account, if any.</param>
        /// <returns><see langword="true"/> if the request is authenticated; <see langword="false"/> otherwise.</returns>
        public bool IsAuthenticated(string authenticationScheme, string requiredAuthenticationScheme, IAccount account)
        {
            if (account == null)
            {
                return false;
            }

            bool requireSpecificAuthenticationScheme =
                !string.IsNullOrEmpty(requiredAuthenticationScheme)
                && !AnyScheme.Equals(requiredAuthenticationScheme);

            if (requireSpecificAuthenticationScheme && !requiredAuthenticationScheme.Equals(authenticationScheme, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Redirects or responds to an unauthorized request.
        /// </summary>
        /// <remarks>Uses the Actions passed to the <see cref="RouteProtector"/> to execute this logic in a framework-agnostic way.</remarks>
        /// <param name="acceptHeader">The HTTP <c>Accept</c> header of this request.</param>
        /// <param name="requestPath">The OWIN request path of this request.</param>
        public void OnUnauthorized(string acceptHeader, string requestPath)
        {
            deleteCookie(webConfiguration.AccessTokenCookie);
            deleteCookie(webConfiguration.RefreshTokenCookie);

            var contentNegotiationResult = ContentNegotiation.NegotiateAcceptHeader(acceptHeader, webConfiguration.Produces, logger);

            bool isHtmlRequest = contentNegotiationResult.Success && contentNegotiationResult.ContentType == ContentType.Html;
            if (isHtmlRequest)
            {
                var loginUri = $"{webConfiguration.Login.Uri}?next={Uri.EscapeUriString(requestPath)}";

                setStatusCode(302);
                redirect(loginUri);
            }
            else
            {
                setStatusCode(401);
                setHeader("WWW-Authenticate", $"Bearer realm=\"{appConfiguration.Name}\"");
            }
        }
    }
}
