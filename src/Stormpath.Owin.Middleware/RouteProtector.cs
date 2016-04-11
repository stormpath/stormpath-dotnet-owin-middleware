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
using Stormpath.Configuration.Abstractions.Model;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Account;

namespace Stormpath.Owin.Middleware
{
    /// <summary>
    /// Represents the logic that protects routes and responds to unauthorized access.
    /// </summary>
    public sealed class RouteProtector
    {
        public static string AnyScheme = "*";

        private readonly WebConfiguration webConfiguration;
        private readonly Action<WebCookieConfiguration> deleteCookie;
        private readonly Action<int> setStatusCode;
        private readonly Action<string> redirect;

        /// <summary>
        /// Creates a new instance of the <see cref="RouteProtector"/> class.
        /// </summary>
        /// <param name="webConfiguration">The active Stormpath <see cref="WebConfiguration">web configuration</see>.</param>
        /// <param name="deleteCookieAction">The routine to run to delete cookies in the response.</param>
        /// <param name="setStatusCodeAction">The routine to run to set the response status code.</param>
        /// <param name="redirectAction">The routine to run to set the response Location header.</param>
        public RouteProtector(
            WebConfiguration webConfiguration,
            Action<WebCookieConfiguration> deleteCookieAction,
            Action<int> setStatusCodeAction,
            Action<string> redirectAction)
        {
            this.webConfiguration = webConfiguration;

            this.deleteCookie = deleteCookieAction;
            this.setStatusCode = setStatusCodeAction;
            this.redirect = redirectAction;
        }

        /// <summary>
        /// Checks whether a properly-authenticated user is making this request.
        /// </summary>
        /// <remarks><paramref name="authenticationScheme"/> and <paramref name="account"/> are available in the OWIN environment as <see cref="Common.OwinKeys.StormpathUser"/> and <see cref="Common.OwinKeys.StormpathUserScheme"/>, respectively.</remarks>
        /// <param name="authenticationScheme">The authentication scheme used for the request.</param>
        /// <param name="requiredAuthenticationScheme">The authentication scheme that must be used for this route, or <see cref="AnyScheme"/>.</param>
        /// <param name="account">The </param>
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

            var contentNegotiationResult = ContentNegotiation.Negotiate(acceptHeader, webConfiguration.Produces);

            bool isHtmlRequest = contentNegotiationResult.Success && contentNegotiationResult.Preferred == ContentType.Html;
            if (isHtmlRequest)
            {
                var loginUri = $"{webConfiguration.Login.Uri}?next={Uri.EscapeUriString(requestPath)}";

                setStatusCode(302);
                redirect(loginUri);
            }
            else
            {
                setStatusCode(401);
            }
        }
    }
}
