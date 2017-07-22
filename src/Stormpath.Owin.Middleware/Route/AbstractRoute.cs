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
using Microsoft.Extensions.Logging;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware.Route
{
    public abstract class AbstractRoute
    {
        private bool _initialized;
        protected IntegrationConfiguration _configuration;
        protected HandlerConfiguration _handlers;
        private IViewRenderer _viewRenderer;
        protected ILogger _logger;
        protected RouteOptionsBase _options;
        protected IOktaClient _oktaClient;

        public void Initialize(
            IntegrationConfiguration configuration,
            HandlerConfiguration handlers,
            IViewRenderer viewRenderer,
            ILogger logger,
            RouteOptionsBase options,
            IOktaClient oktaClient)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _viewRenderer = viewRenderer ?? throw new ArgumentNullException(nameof(viewRenderer));
            _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
            _logger = logger;
            _options = options;
            _oktaClient = oktaClient ?? throw new ArgumentNullException(nameof(oktaClient));

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
                _logger.LogTrace($"Content negotiation failed for request {owinContext.Request.Path}. Skipping", nameof(InvokeAsync));
                return false;
            }

            try
            {
                return await HandleRequestAsync(owinContext, contentNegotiationResult, owinContext.CancellationToken);
            }
            catch (Exception ex)
            {
                if (contentNegotiationResult.ContentType == ContentType.Json)
                {
                    // Sanitize framework-level errors
                    await Error.Create(owinContext, 500, ex.Message, owinContext.CancellationToken);
                    return true;
                }
                else
                {
                    // TODO return HTML?
                    await Error.Create(owinContext, 500, ex.Message, owinContext.CancellationToken);
                    return true;
                }
            }
        }

        private Task<bool> HandleRequestAsync(IOwinEnvironment context, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
        {
            var method = context.Request.Method;

            if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                return GetAsync(context, contentNegotiationResult, cancellationToken);
            }

            if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                return PostAsync(context, contentNegotiationResult, cancellationToken);
            }

            // Do nothing and pass on to next middleware.
            _logger.LogTrace("Request method was not GET or POST", nameof(HandleRequestAsync));
            return Task.FromResult(false);
        }

        protected Task RenderViewAsync(IOwinEnvironment context, string viewName, object model, CancellationToken cancellationToken)
        {
            context.Response.StatusCode = 200;
            context.Response.Headers.SetString("Content-Type", Constants.HtmlContentType);

            return _viewRenderer.RenderAsync(viewName, model, context, cancellationToken);
        }

        protected virtual Task<bool> GetAsync(IOwinEnvironment context, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
        {
            if (contentNegotiationResult.ContentType == ContentType.Json)
            {
                return GetJsonAsync(context, cancellationToken);
            }

            if (contentNegotiationResult.ContentType == ContentType.Html)
            {
                return GetHtmlAsync(context, cancellationToken);
            }

            // Do nothing and pass on to next middleware.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> PostAsync(IOwinEnvironment context, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
        {
            var rawBodyContentType = context.Request.Headers.GetString("Content-Type");
            var bodyContentTypeDetectionResult = ContentNegotiation.DetectBodyType(rawBodyContentType);

            if (!bodyContentTypeDetectionResult.Success)
            {
                throw new Exception($"The Content-Type '{rawBodyContentType}' is invalid.");
            }

            if (contentNegotiationResult.ContentType == ContentType.Json)
            {
                return PostJsonAsync(context, bodyContentTypeDetectionResult.ContentType, cancellationToken);
            }

            if (contentNegotiationResult.ContentType == ContentType.Html)
            {
                return PostHtmlAsync(context, bodyContentTypeDetectionResult.ContentType, cancellationToken);
            }

            // Do nothing and pass on to next middleware.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> GetJsonAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> GetHtmlAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> PostJsonAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> PostHtmlAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }
    }
}
