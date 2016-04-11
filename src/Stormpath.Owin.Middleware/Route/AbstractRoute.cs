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
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Common;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Route
{
    using Renderer = Func<string, object, IOwinEnvironment, System.Threading.CancellationToken, Task>;

    public abstract class AbstractRoute
    {
        private bool _initialized;
        protected StormpathConfiguration _configuration;
        private Renderer _viewRenderer;
        protected ILogger _logger;
        private IClient _client;

        public void Initialize(
            StormpathConfiguration configuration,
            Renderer viewRenderer,
            ILogger logger,
            IClient client)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (viewRenderer == null)
            {
                throw new ArgumentNullException(nameof(viewRenderer));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }


            _configuration = configuration;
            _viewRenderer = viewRenderer;
            _logger = logger;
            _client = client;

            _initialized = true;
        }

        public async Task<bool> InvokeAsync(IOwinEnvironment owinContext)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Route has not been initialized.");
            }

            var acceptHeader = owinContext.Request.Headers.GetString("Accept");
            var contentNegotiationResult = ContentNegotiation.Negotiate(acceptHeader, _configuration.Web.Produces);

            if (!contentNegotiationResult.Success)
            {
                return false;
            }

            _logger.Info($"Stormpath middleware handling request {owinContext.Request.Path}");

            try
            {
                return await DispatchAsync(owinContext, _client, contentNegotiationResult, owinContext.CancellationToken);
            }
            catch (ResourceException rex)
            {
                if (contentNegotiationResult.Preferred == ContentType.Json)
                {
                    // Sanitize Stormpath API errors
                    await Error.CreateFromApiError(owinContext, rex, owinContext.CancellationToken);
                    return true;
                }
                else
                {
                    // todo
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (contentNegotiationResult.Preferred == ContentType.Json)
                {
                    // Sanitize framework-level errors
                    await Error.Create(owinContext, 400, ex.Message, owinContext.CancellationToken);
                    return true;
                }
                else
                {
                    // todo
                    throw;
                }
            }
        }

        private Task<bool> DispatchAsync(IOwinEnvironment context, IClient scopedClient, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
        {
            var method = context.Request.Method;

            if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                return GetAsync(context, scopedClient, contentNegotiationResult, cancellationToken);
            }

            if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                return PostAsync(context, scopedClient, contentNegotiationResult, cancellationToken);
            }

            // Do nothing and pass on to next middleware.
            return Task.FromResult(false);
        }

        protected Task RenderViewAsync(IOwinEnvironment context, string viewName, object model, CancellationToken cancellationToken)
        {
            context.Response.StatusCode = 200;
            context.Response.Headers.SetString("Content-Type", Constants.HtmlContentType);

            return _viewRenderer(viewName, model, context, cancellationToken);
        }

        protected virtual Task<bool> GetAsync(IOwinEnvironment context, IClient client, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
        {
            if (contentNegotiationResult.Preferred == ContentType.Json)
            {
                return GetJsonAsync(context, client, cancellationToken);
            }

            if (contentNegotiationResult.Preferred == ContentType.Html)
            {
                return GetHtmlAsync(context, client, cancellationToken);
            }

            // Do nothing and pass on to next middleware.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> PostAsync(IOwinEnvironment context, IClient client, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
        {
            if (contentNegotiationResult.Preferred == ContentType.Json)
            {
                return PostJsonAsync(context, client, cancellationToken);
            }

            if (contentNegotiationResult.Preferred == ContentType.Html)
            {
                return PostHtmlAsync(context, client, cancellationToken);
            }

            // Do nothing and pass on to next middleware.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> GetJsonAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> PostJsonAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> PostHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }
    }
}
