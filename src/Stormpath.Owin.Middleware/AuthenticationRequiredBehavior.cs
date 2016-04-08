// <copyright file="StormpathMiddleware.GetUser.cs" company="Stormpath, Inc.">
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
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions.Model;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Account;

namespace Stormpath.Owin.Middleware
{
    public sealed class AuthenticationRequiredBehavior
    {
        public static string AnyScheme = "*";

        private readonly WebConfiguration webConfiguration;
        private readonly Func<string> getAcceptHeader;
        private readonly Func<string> getRequestPath;
        private readonly Action<string> deleteCookie;
        private readonly Action<int> setStatusCode;
        private readonly Action<string> redirect;

        public AuthenticationRequiredBehavior(
            WebConfiguration webConfiguration,
            Func<string> getAcceptHeaderFunc,
            Func<string> getRequestPathFunc,
            Action<string> deleteCookieAction,
            Action<int> setStatusCodeAction,
            Action<string> redirectAction)
        {
            this.webConfiguration = webConfiguration;

            this.getAcceptHeader = getAcceptHeaderFunc;
            this.getRequestPath = getRequestPathFunc;
            this.deleteCookie = deleteCookieAction;
            this.setStatusCode = setStatusCodeAction;
            this.redirect = redirectAction;
        }

        public bool IsAuthorized(string authenticationScheme, string requiredAuthenticationScheme, IAccount account)
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

        public void OnUnauthorized()
        {
            deleteCookie(webConfiguration.AccessTokenCookie.Name);
            deleteCookie(webConfiguration.RefreshTokenCookie.Name);

            var contentNegotiationResult = ContentNegotiation.Negotiate(getAcceptHeader(), webConfiguration.Produces);

            bool isHtmlRequest = contentNegotiationResult.Success && contentNegotiationResult.Preferred == ContentType.Html;
            if (isHtmlRequest)
            {
                var originalUri = getRequestPath(); // todo ensure it's a path
                var loginUri = $"{webConfiguration.Login.Uri}?next={Uri.EscapeUriString(originalUri)}";

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
