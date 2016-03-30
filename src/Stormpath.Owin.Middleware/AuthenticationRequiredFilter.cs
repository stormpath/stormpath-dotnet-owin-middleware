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
using System.Collections.Generic;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Common;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.SDK.Account;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware
{
    public class AuthenticationRequiredFilter
    {
        private readonly ILogger logger;

        public AuthenticationRequiredFilter()
            : this(null)
        {
        }

        public AuthenticationRequiredFilter(ILogger logger = null)
        {
            this.logger = logger;
        }

        public async Task<bool> Invoke(IDictionary<string, object> environment)
        {
            IOwinEnvironment context = new DefaultOwinEnvironment(environment);
            var stormpathUser = environment.Get<IAccount>(OwinKeys.StormpathUser);

            if (stormpathUser != null)
            {
                return true; // Authentication check succeeded
            }

            logger.Info("User attempted to access a protected endpoint with invalid credentials.");

            var configuration = environment.Get<StormpathConfiguration>(OwinKeys.StormpathConfiguration);

            Cookies.DeleteTokenCookies(context, configuration.Web);

            var acceptHeader = context.Request.Headers.GetString("Accept");
            var contentNegotiationResult = ContentNegotiation.Negotiate(acceptHeader, configuration.Web.Produces);

            if (contentNegotiationResult.Success && contentNegotiationResult.Preferred == ContentType.Html)
            {
                context.Response.StatusCode = 302;

                var originalUri = context.Request.OriginalUri;
                var loginUri = $"{configuration.Web.Login.Uri}?next={Uri.EscapeUriString(originalUri)}";
                context.Response.Headers.SetString("Location", loginUri);

                return false; // Authentication check failed
            }

            await JsonResponse.Unauthorized(context);
            return false; // Authentication check failed
        }
    }
}