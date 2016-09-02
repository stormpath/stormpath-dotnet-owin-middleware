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
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Account;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware
{
    public sealed class AuthenticationRequiredFilter
    {
        private readonly ILogger logger;

        public AuthenticationRequiredFilter(ILogger logger)
        {
            this.logger = logger;
        }

        public Task<bool> InvokeAsync(IDictionary<string, object> environment)
        {
            IOwinEnvironment context = new DefaultOwinEnvironment(environment);
            var configuration = environment.Get<StormpathConfiguration>(OwinKeys.StormpathConfiguration);
            var authenticatedUser = environment.Get<IAccount>(OwinKeys.StormpathUser);
            var authenticationScheme = environment.Get<string>(OwinKeys.StormpathUserScheme);

            var deleteCookieAction = new Action<WebCookieConfiguration>(cookie => Cookies.DeleteTokenCookie(context, cookie, logger));
            var setStatusCodeAction = new Action<int>(code => context.Response.StatusCode = code);
            var setHeaderAction = new Action<string, string>((name, value) => context.Response.Headers.SetString(name, value));
            var redirectAction = new Action<string>(location => context.Response.Headers.SetString("Location", location));

            var handler = new RouteProtector(
                configuration.Application,
                configuration.Web,
                deleteCookieAction,
                setStatusCodeAction,
                setHeaderAction,
                redirectAction,
                logger);

            if (handler.IsAuthenticated(authenticationScheme, RouteProtector.AnyScheme, authenticatedUser))
            {
                return TaskConstants.CompletedTask; // Authentication check succeeded
            }

            logger.Info("User attempted to access a protected endpoint with invalid credentials.");

            handler.OnUnauthorized(context.Request.Headers.GetString("Accept"), context.Request.Path);
            return Task.FromResult(false); // Authentication check failed
        }
    }
}