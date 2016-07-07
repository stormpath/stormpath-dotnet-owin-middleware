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
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Route;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.SDK.Account;

namespace Stormpath.Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public sealed partial class StormpathMiddleware
    {
        private readonly IViewRenderer viewRenderer;
        private readonly ILogger logger;
        private readonly IFrameworkUserAgentBuilder userAgentBuilder;
        private readonly IScopedClientFactory clientFactory;
        private readonly IReadOnlyDictionary<string, RouteHandler> routingTable;
        private AppFunc next;

        private StormpathMiddleware(
            IViewRenderer viewRenderer,
            ILogger logger,
            IFrameworkUserAgentBuilder userAgentBuilder,
            IScopedClientFactory clientFactory,
            IntegrationConfiguration configuration)
        {
            this.viewRenderer = viewRenderer;
            this.logger = logger;
            this.userAgentBuilder = userAgentBuilder;
            this.clientFactory = clientFactory;
            this.Configuration = configuration;

            this.routingTable = this.BuildRoutingTable();
        }

        public IntegrationConfiguration Configuration { get; }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (this.next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            IOwinEnvironment context = new DefaultOwinEnvironment(environment);
            logger.Trace($"Incoming request {context.Request.Path}", "StormpathMiddleware.Invoke");

            using (var scopedClient = this.CreateScopedClient(context))
            {
                var currentUser = await GetUserAsync(context, scopedClient);

                if (currentUser == null)
                {
                    logger.Trace("Request is anonymous", "StormpathMiddleware.Invoke");
                }
                else
                {
                    logger.Trace($"Request for Account '{currentUser.Href}'", "StormpathMiddleware.Invoke");
                }

                AddStormpathVariablesToEnvironment(
                    environment,
                    Configuration,
                    scopedClient,
                    currentUser);

                var requestPath = GetRequestPathOrThrow(context);
                var routeHandler = GetRouteHandler(requestPath);

                if (routeHandler == null)
                {
                    await this.next.Invoke(environment);
                    return;
                }

                logger.Trace($"Handling request '{requestPath}'", "StormpathMiddleware.Invoke");

                if (routeHandler.AuthenticationRequired)
                {
                    var filter = new AuthenticationRequiredFilter(this.logger);
                    var isAuthenticated = await filter.InvokeAsync(environment);
                    if (!isAuthenticated)
                    {
                        return;
                    }
                }

                var handled = await routeHandler.Handler(scopedClient)(context);

                if (!handled)
                {
                    logger.Trace("Handler skipped request.", "StormpathMiddleware.Invoke");
                    await this.next.Invoke(environment);
                }
            }
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
            IClient client,
            IAccount currentUser)
        {
            environment[OwinKeys.StormpathConfiguration] = configuration;
            environment[OwinKeys.StormpathClient] = client;

            if (currentUser != null)
            {
                environment[OwinKeys.StormpathUser] = currentUser;
            }
        }

        private IClient CreateScopedClient(IOwinEnvironment context)
        {
            var fullUserAgent = CreateFullUserAgent(context);

            var scopedClientOptions = new ScopedClientOptions()
            {
                UserAgent = fullUserAgent
            };

            return clientFactory.Create(scopedClientOptions);
        }

        private string CreateFullUserAgent(IOwinEnvironment context)
        {
            var callingAgent = string
                .Join(" ", context.Request.Headers.Get("X-Stormpath-Agent") ?? new string[0])
                .Trim();

            return string
                .Join(" ", callingAgent, userAgentBuilder.GetUserAgent())
                .Trim();
        }
    }
}
