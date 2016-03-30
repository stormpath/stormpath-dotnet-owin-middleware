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
using Stormpath.Owin.Middleware.Owin;
using Stormpath.Owin.Middleware.Route;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware
{
    using Common;
    using Common.Configuration;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public sealed partial class StormpathMiddleware
    {
        private readonly ILogger logger = null;
        private readonly IFrameworkUserAgentBuilder userAgentBuilder;
        private readonly IScopedClientFactory clientFactory;
        private readonly IntegrationConfiguration configuration;
        private readonly IReadOnlyDictionary<string, RouteHandler> routingTable;
        private AppFunc next;

        private StormpathMiddleware(
            ILogger logger,
            IFrameworkUserAgentBuilder userAgentBuilder,
            IScopedClientFactory clientFactory,
            IntegrationConfiguration configuration)
        {
            this.logger = logger;
            this.userAgentBuilder = userAgentBuilder;
            this.clientFactory = clientFactory;
            this.configuration = configuration;

            this.routingTable = this.BuildRoutingTable();
        }

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

            AddStormpathVariablesToEnvironment(environment);

            IOwinEnvironment context = new DefaultOwinEnvironment(environment);

            using (var scopedClient = this.CreateScopedClient(context))
            {
                await GetUserAsync(context, scopedClient);

                var requestPath = GetRequestPathOrThrow(context);
                var routeHandler = GetRouteHandler(requestPath);

                if (routeHandler == null)
                {
                    await this.next.Invoke(environment);
                    return;
                }

                if (routeHandler.AuthenticationRequired)
                {
                    var filter = new AuthenticationRequiredFilter(this.logger);
                    var isAuthenticated = await filter.Invoke(environment);
                    if (!isAuthenticated)
                    {
                        return;
                    }
                }

                var handled = await routeHandler.Handler(scopedClient)(context);

                if (!handled)
                {
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
            routingTable.TryGetValue(requestPath, out handler);
            return handler;
        }

        private void AddStormpathVariablesToEnvironment(IDictionary<string, object> environment)
        {
            environment[OwinKeys.StormpathConfiguration] = this.configuration;
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
