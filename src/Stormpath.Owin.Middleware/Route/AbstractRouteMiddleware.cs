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
    using SDK.Error;

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
            _supportedContentTypes = _configuration.Web.Produces.Intersect(supportedContentTypes).ToArray();
        }

        public async Task<bool> Invoke(IOwinEnvironment owinContext)
        {
            if (!IsSupportedVerb(owinContext))
            {
                await Error.Create<MethodNotAllowed>(owinContext, owinContext.CancellationToken);
                return true;
            }

            if (!HasSupportedAccept(owinContext))
            {
                await Error.Create<NotAcceptable>(owinContext, owinContext.CancellationToken);
                return true;
            }

            _logger.Info($"Stormpath middleware handling request {owinContext.Request.Path}");

            var targetContentType = ContentNegotiation.SelectBestContentType(owinContext, _supportedContentTypes);

            try
            {
                return await Dispatch(owinContext, _client, targetContentType, owinContext.CancellationToken);
            }
            catch (ResourceException rex)
            {
                if (targetContentType == "application/json")
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
                if (targetContentType == "application/json")
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

        private bool IsSupportedVerb(IOwinEnvironment context)
            => _supportedMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase);

        private bool HasSupportedAccept(IOwinEnvironment context)
            => true; //todo

        private Task<bool> Dispatch(IOwinEnvironment context, IClient scopedClient, string targetContentType, CancellationToken cancellationToken)
        {
            var method = context.Request.Method;

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

            // todo: probably remove this
            throw new Exception($"Unknown target Content-Type: '{targetContentType}'.");
        }

        protected virtual Task<bool> GetJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> GetHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> PostJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }

        protected virtual Task<bool> PostHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // Do nothing and pass on to next middleware by default.
            return Task.FromResult(false);
        }
    }
}
