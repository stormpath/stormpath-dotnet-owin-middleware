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
using System.Linq;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions;
using Stormpath.Configuration.Abstractions.Model;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Route;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.SDK;
using Stormpath.SDK.Client;
using Stormpath.SDK.Http;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Serialization;
using Stormpath.SDK.Sync;

namespace Stormpath.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using RouteHandler = Func<IClient, Func<IOwinEnvironment, Task>>;

    public sealed partial class StormpathMiddleware
    {
        private readonly ILogger logger = null;
        private readonly IFrameworkUserAgentBuilder userAgentBuilder;
        private readonly IScopedClientFactory clientFactory;
        private readonly StormpathConfiguration configuration;
        private readonly IReadOnlyDictionary<string, RouteHandler> routingTable;
        private AppFunc next;

        private StormpathMiddleware(
            ILogger logger,
            IFrameworkUserAgentBuilder userAgentBuilder,
            IScopedClientFactory clientFactory,
            StormpathConfiguration configuration)
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

        public Task Invoke(IDictionary<string, object> environment)
        {
            if (this.next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            var requestPath = GetRequestPathOrThrow(environment);
            var routeHandler = GetRouteHandler(requestPath);

            if (routeHandler == null)
            {
                return this.next.Invoke(environment);
            }

            IOwinEnvironment owinContext = new DefaultOwinEnvironment(environment);

            if (!ContentNegotiation.IsSupportedByConfiguration(owinContext, this.configuration))
            {
                owinContext.Response.StatusCode = 406;
                return Task.FromResult(0);
            }

            using (var scopedClient = this.CreateScopedClient(owinContext))
            {
                return routeHandler(scopedClient)(owinContext);
            }
        }

        private static string GetRequestPathOrThrow(IDictionary<string, object> environment)
        {
            object requestPathRaw;

            if (!environment.TryGetValue(OwinKeys.RequestPath, out requestPathRaw))
            {
                throw new Exception($"Invalid OWIN request. Expected {OwinKeys.RequestPath}, but it was not found.");
            }

            return requestPathRaw.ToString();
        }

        private RouteHandler GetRouteHandler(string requestPath)
        {
            RouteHandler handler = null;
            routingTable.TryGetValue(requestPath, out handler);
            return handler;
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

        private IReadOnlyDictionary<string, RouteHandler> BuildRoutingTable()
        {
            var routingTable = new Dictionary<string, RouteHandler>();

            if (this.configuration.Web.Oauth2.Enabled == true)
            {
                routingTable.Add(
                    this.configuration.Web.Oauth2.Uri,
                    client => new Oauth2Route(this.configuration, this.logger, client).Invoke);
            }

            if (this.configuration.Web.Register.Enabled == true)
            {
                routingTable.Add(
                    this.configuration.Web.Register.Uri,
                    client => new RegisterRoute(this.configuration, this.logger, client).Invoke);
            }

            if (this.configuration.Web.Login.Enabled == true)
            {
                routingTable.Add(
                    this.configuration.Web.Login.Uri,
                    client => new LoginRoute(this.configuration, this.logger, client).Invoke);
            }

            return routingTable;
        }
    }
}
