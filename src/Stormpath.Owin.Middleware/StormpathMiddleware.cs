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

    public sealed partial class StormpathMiddleware
    {
        private readonly ILogger logger = null;
        private readonly IFrameworkUserAgentBuilder userAgentBuilder;
        private readonly IScopedClientFactory clientFactory;
        private readonly StormpathConfiguration configuration;
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

            object requestPathRaw;

            if (!environment.TryGetValue(OwinKeys.RequestPath, out requestPathRaw))
            {
                throw new Exception($"Invalid OWIN request. Expected {OwinKeys.RequestPath}, but it was not found.");
            }

            var requestPath = requestPathRaw.ToString();
            var routeHandler = GetRouteHandler(requestPath);

            return routeHandler == null
                ? this.next.Invoke(environment)
                : routeHandler(environment);
        }

        private Func<IDictionary<string, object>, Task> GetRouteHandler(string requestPath)
        {
            return null;
        }
    }
}
