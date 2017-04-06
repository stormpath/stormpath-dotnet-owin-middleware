// <copyright file="StormpathMiddleware.cs" company="Stormpath, Inc.">
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
using Microsoft.Extensions.Logging;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Route;

using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public sealed partial class StormpathMiddleware
    {
        private readonly IOktaClient oktaClient;
        private readonly IKeyProvider keyProvider;
        private readonly IViewRenderer viewRenderer;
        private readonly ILogger logger;
        private readonly IFrameworkUserAgentBuilder userAgentBuilder;
        private readonly IReadOnlyDictionary<string, RouteHandler> routingTable;
        private AppFunc _next;

        private StormpathMiddleware(
            IOktaClient oktaClient,
            IKeyProvider keyProvider,
            IViewRenderer viewRenderer,
            ILogger logger,
            IFrameworkUserAgentBuilder userAgentBuilder,
            IntegrationConfiguration configuration,
            HandlerConfiguration handlers)
        {
            this.oktaClient = oktaClient;
            this.keyProvider = keyProvider;
            this.viewRenderer = viewRenderer;
            this.logger = logger;
            this.userAgentBuilder = userAgentBuilder;
            this.Configuration = configuration;
            this.Handlers = handlers;

            this.routingTable = this.BuildRoutingTable();
        }

        public IntegrationConfiguration Configuration { get; }

        public HandlerConfiguration Handlers { get; }

        public void Initialize(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (this._next == null)
            {
                throw new ArgumentNullException(nameof(_next));
            }

            IOwinEnvironment context = new DefaultOwinEnvironment(environment);
            logger.LogTrace($"Incoming request {context.Request.Path}", "StormpathMiddleware.Invoke");

            //using (var scopedClient = CreateScopedClient(context))
            //{
                var currentUser = await GetUserAsync(context, context.CancellationToken).ConfigureAwait(false);

                if (currentUser == null)
                {
                    logger.LogTrace("Request is anonymous", "StormpathMiddleware.Invoke");
                }
                else
                {
                    logger.LogTrace($"Request for Account '{currentUser.Href}' via scheme {environment[OwinKeys.StormpathUserScheme]}", "StormpathMiddleware.Invoke");
                }

                AddStormpathVariablesToEnvironment(
                    environment,
                    Configuration,
                    currentUser);

                var requestPath = GetRequestPathOrThrow(context);
                var routeHandler = GetRouteHandler(requestPath);

                if (routeHandler == null)
                {
                    await this._next.Invoke(environment);
                    return;
                }

                logger.LogTrace($"Handling request '{requestPath}'", "StormpathMiddleware.Invoke");

                if (routeHandler.AuthenticationRequired)
                {
                    var filter = new AuthenticationRequiredFilter(this.logger);
                    var isAuthenticated = await filter.InvokeAsync(environment);
                    if (!isAuthenticated)
                    {
                        return;
                    }
                }

                var handled = await routeHandler.Handler()(context);

                if (!handled)
                {
                    logger.LogTrace("Handler skipped request.", "StormpathMiddleware.Invoke");
                    await this._next.Invoke(environment);
                }
            //}
        }

        private static string GetRequestPathOrThrow(IOwinEnvironment context)
        {
            var requestPath = context.Request.Path;

            if (string.IsNullOrEmpty(requestPath))
            {
                throw new Exception($"Invalid OWIN request. Expected {OwinKeys.RequestPath}, but it was not found.");
            }

            return requestPath;
        }

        private RouteHandler GetRouteHandler(string requestPath)
        {
            RouteHandler handler = null;
            this.routingTable.TryGetValue(requestPath, out handler);
            return handler;
        }

        private static void AddStormpathVariablesToEnvironment(
            IDictionary<string, object> environment,
            StormpathConfiguration configuration,
            dynamic currentUser)
        {
            environment[OwinKeys.StormpathConfiguration] = configuration;

            if (currentUser != null)
            {
                environment[OwinKeys.StormpathUser] = currentUser;
            }
        }

        private string CreateFullUserAgent(IOwinEnvironment context)
        {
            var callingAgent = string.Empty;

            if (context != null)
            {
                callingAgent = string
                .Join(" ", context.Request.Headers.Get("X-Stormpath-Agent") ?? new string[0])
                .Trim();
            }

            return string
                .Join(" ", callingAgent, userAgentBuilder.GetUserAgent())
                .Trim();
        }
    }
}
