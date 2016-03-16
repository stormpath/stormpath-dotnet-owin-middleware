// <copyright file="AbstractMiddlewareController.cs" company="Stormpath, Inc.">
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
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.Configuration.Abstractions;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Route
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public abstract class AbstractRouteMiddleware
    {
        private readonly IClient _client;
        private readonly string[] _supportedMethods;
        private readonly string[] _supportedContentTypes;

        protected readonly ILogger _logger;
        protected readonly StormpathConfiguration _configuration;

        public AbstractRouteMiddleware(
            StormpathConfiguration configuration,
            ILogger logger,
            IClient client,
            IEnumerable<string> supportedMethods,
            IEnumerable<string> supportedContentTypes)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _logger = logger;
            _configuration = configuration;
            _client = client;
            _supportedMethods = supportedMethods.ToArray();
            _supportedContentTypes = supportedContentTypes.ToArray();
        }

        public Task Invoke(IOwinEnvironment owinContext)
        {
            if (!IsSupportedVerb(owinContext))
            {
                return Error.Create<MethodNotAllowed>(owinContext);
            }

            if (!HasSupportedAccept(owinContext))
            {
                return Error.Create<NotAcceptable>(owinContext);
            }

            _logger.Info($"Stormpath middleware handling request {owinContext.Request.Path}");

            return Dispatch(owinContext, _client, owinContext.CancellationToken);
        }

        private bool IsSupportedVerb(IOwinEnvironment context)
            => _supportedMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase);

        private bool HasSupportedAccept(IOwinEnvironment context)
            => true; //todo

        private Task Dispatch(IOwinEnvironment context, IClient scopedClient, CancellationToken cancellationToken)
        {
            var method = context.Request.Method;
            var targetContentType = ContentNegotiation.SelectBestContentType(context, _supportedContentTypes);

            if (targetContentType == "application/json")
            {
                if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    return GetJson(context, scopedClient, cancellationToken);
                }

                if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return PostJson(context, scopedClient, cancellationToken);
                }

                throw new Exception($"Unknown verb to Stormpath middleware: '{method}'.");
            }
            else if (targetContentType == "text/html")
            {
                if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    return GetHtml(context, scopedClient, cancellationToken);
                }

                if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return PostHtml(context, scopedClient, cancellationToken);
                }

                throw new Exception($"Unknown verb to Stormpath middleware: '{method}'.");
            }

            throw new Exception($"Unknown target Content-Type: '{targetContentType}'.");
        }

        protected virtual Task GetJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // This should not happen with proper configuration.
            throw new NotImplementedException("Fatal error: this controller does not support GET with application/json.");
        }

        protected virtual Task GetHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // This should not happen with proper configuration.
            throw new NotImplementedException("Fatal error: this controller does not support GET with text/html.");
        }

        protected virtual Task PostJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // This should not happen with proper configuration.
            throw new NotImplementedException("Fatal error: this controller does not support POST with application/json.");
        }

        protected virtual Task PostHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // This should not happen with proper configuration.
            throw new NotImplementedException("Fatal error: this controller does not support POST with text/html.");
        }
    }
}
