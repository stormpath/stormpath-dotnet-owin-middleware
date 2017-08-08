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
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Okta;
using Stormpath.Owin.Middleware.Route;

namespace Stormpath.Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public sealed partial class StormpathMiddleware
    {
        private readonly IKeyProvider _keyProvider;
        private readonly IViewRenderer _viewRenderer;
        private readonly ILogger _logger;
        private readonly IFrameworkUserAgentBuilder _userAgentBuilder;
        private readonly IReadOnlyDictionary<string, RouteHandler> _routingTable;
        private AppFunc _next;

        private StormpathMiddleware(
            IKeyProvider keyProvider,
            IViewRenderer viewRenderer,
            ILogger logger,
            IFrameworkUserAgentBuilder userAgentBuilder,
            IntegrationConfiguration configuration,
            HandlerConfiguration handlers,
            IAuthorizationFilterFactory authorizationFilterFactory,
            OktaClient oktaClient)
        {
            _keyProvider = keyProvider;
            _viewRenderer = viewRenderer;
            _logger = logger;
            _userAgentBuilder = userAgentBuilder;

            Configuration = configuration;
            Handlers = handlers;
            AuthorizationFilterFactory = authorizationFilterFactory;

            Client = oktaClient;

            _routingTable = BuildRoutingTable();
        }

        public IntegrationConfiguration Configuration { get; }

        public IAuthorizationFilterFactory AuthorizationFilterFactory { get; }

        private HandlerConfiguration Handlers { get; }

        private OktaClient Client { get; }

        public void Initialize(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (_next == null)
            {
                throw new ArgumentNullException(nameof(_next));
            }

            IOwinEnvironment context = new DefaultOwinEnvironment(environment);
            _logger.LogTrace($"Incoming request {context.Request.Path}", "StormpathMiddleware.Invoke");

            var currentUser = await GetUserAsync(context, context.CancellationToken).ConfigureAwait(false);

            if (currentUser == null)
            {
                _logger.LogTrace("Request is anonymous", "StormpathMiddleware.Invoke");
            }
            else
            {
                _logger.LogTrace($"Request for Account '{currentUser.Href}' via scheme {environment[OwinKeys.StormpathUserScheme]}", "StormpathMiddleware.Invoke");
            }

            AddStormpathVariablesToEnvironment(
                environment,
                Configuration,
                AuthorizationFilterFactory,
                currentUser);

            var requestPath = GetRequestPathOrThrow(context);
            var routeHandler = GetRouteHandler(requestPath);

            if (routeHandler == null)
            {
                await _next.Invoke(environment);
                return;
            }

            _logger.LogTrace($"Handling request '{requestPath}'", "StormpathMiddleware.Invoke");

            if (routeHandler.AuthenticationRequired)
            {
                var filter = new AuthenticationRequiredFilter(_logger);
                var isAuthenticated = await filter.InvokeAsync(environment);
                if (!isAuthenticated)
                {
                    return;
                }
            }

            var handled = await routeHandler.Handler()(context);

            if (!handled)
            {
                _logger.LogTrace("Handler skipped request.", "StormpathMiddleware.Invoke");
                await _next.Invoke(environment);
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
            _routingTable.TryGetValue(requestPath, out handler);
            return handler;
        }

        private static void AddStormpathVariablesToEnvironment(
            IDictionary<string, object> environment,
            StormpathConfiguration configuration,
            IAuthorizationFilterFactory authorizationFilterFactory,
            ICompatibleOktaAccount currentUser)
        {
            environment[OwinKeys.StormpathConfiguration] = configuration;
            environment[OwinKeys.StormpathAuthorizationFilterFactory] = authorizationFilterFactory;

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
                .Join(" ", callingAgent, _userAgentBuilder.GetUserAgent())
                .Trim();
        }
    }
}
