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
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Route
{
    public abstract class AbstractRoute
    {
        private bool _initialized;
        protected IntegrationConfiguration _configuration;
        protected HandlerConfiguration _handlers;
        private IViewRenderer _viewRenderer;
        protected ILogger _logger;
        private IClient _client;

        public void Initialize(
            IntegrationConfiguration configuration,
            HandlerConfiguration handlers,
            IViewRenderer viewRenderer,
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

            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }

            _configuration = configuration;
            _viewRenderer = viewRenderer;
            _logger = logger;
            _client = client;
            _handlers = handlers;

            _initialized = true;
        }

        public async Task<bool> InvokeAsync(IOwinEnvironment owinContext)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Route has not been initialized.");
            }

            var acceptHeader = owinContext.Request.Headers.GetString("Accept");
            var contentNegotiationResult = ContentNegotiation.NegotiateAcceptHeader(acceptHeader, _configuration.Web.Produces, _logger);

            if (!contentNegotiationResult.Success)
            {
                _logger.Trace($"Content negotiation failed for request {owinContext.Request.Path}. Skipping", nameof(InvokeAsync));
                return false;
            }

            try
            {
                return await HandleRequestAsync(owinContext, _client, contentNegotiationResult, owinContext.CancellationToken);
            }
            catch (ResourceException rex)
            {
                if (contentNegotiationResult.ContentType == ContentType.Json)
                {
                    // Sanitize Stormpath API errors
                    await Error.CreateFromApiError(owinContext, rex, owinContext.CancellationToken);
                    return true;
                }
                else
                {
                    // todo handle framework errors
                    _logger.Error(rex, source: nameof(InvokeAsync));
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (contentNegotiationResult.ContentType == ContentType.Json)
                {
                    // Sanitize framework-level errors
                    await Error.Create(owinContext, 400, ex.Message, owinContext.CancellationToken);
                    return true;
                }
                else
                {
                    // todo handle framework errors
                    _logger.Error(ex, source: nameof(InvokeAsync));
                    throw;
                }
            }
        }

        private Task<bool> HandleRequestAsync(IOwinEnvironment context, IClient scopedClient, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
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
            _logger.Trace("Request method was not GET or POST", nameof(HandleRequestAsync));
            return Task.FromResult(false);
        }

        protected Task RenderViewAsync(IOwinEnvironment context, string viewName, object model, CancellationToken cancellationToken)
        {
            context.Response.StatusCode = 200;
            context.Response.Headers.SetString("Content-Type", Constants.HtmlContentType);

            return _viewRenderer.RenderAsync(viewName, model, context, cancellationToken);
        }

        protected virtual Task<bool> GetAsync(IOwinEnvironment context, IClient client, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
        {
            if (contentNegotiationResult.ContentType == ContentType.Json)
            {
                return GetJsonAsync(context, client, cancellationToken);
            }

            if (contentNegotiationResult.ContentType == ContentType.Html)
            {
                return GetHtmlAsync(context, client, cancellationToken);
            }

            // Do nothing and pass on to next middleware.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> PostAsync(IOwinEnvironment context, IClient client, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
        {
            var rawBodyContentType = context.Request.Headers.GetString("Content-Type");
            var bodyContentTypeDetectionResult = ContentNegotiation.DetectBodyType(rawBodyContentType);

            if (!bodyContentTypeDetectionResult.Success)
            {
                throw new Exception($"The Content-Type '{rawBodyContentType}' is invalid.");
            }

            if (contentNegotiationResult.ContentType == ContentType.Json)
            {
                return PostJsonAsync(context, client, bodyContentTypeDetectionResult.ContentType, cancellationToken);
            }

            if (contentNegotiationResult.ContentType == ContentType.Html)
            {
                return PostHtmlAsync(context, client, bodyContentTypeDetectionResult.ContentType, cancellationToken);
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

        protected virtual Task<bool> PostJsonAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> PostHtmlAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }
    }
}
