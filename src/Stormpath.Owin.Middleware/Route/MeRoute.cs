// <copyright file="MeRoute.cs" company="Stormpath, Inc.">
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

using System.Threading;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Common;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Route
{
    public class MeRoute : AbstractRouteMiddleware
    {
        private readonly static string[] SupportedMethods = { "GET" };
        private readonly static string[] SupportedContentTypes = { "application/json" };

        public MeRoute(
            StormpathConfiguration configuration,
            ILogger logger,
            IClient client)
            : base(configuration, logger, client, SupportedMethods, SupportedContentTypes)
        {
        }

        protected override Task GetJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            context.Response.Headers.SetString("Content-Type", Constants.JsonContentType);
            context.Response.Headers.SetString("Cache-Control", "no-store");
            context.Response.Headers.SetString("Pragma", "no-cache");

            var stormpathAccount = context.Request[OwinKeys.StormpathUser] as IAccount;

            var sanitizer = new ResponseSanitizer<IAccount>();
            var responseModel = new
            {
                account = sanitizer.Sanitize(stormpathAccount)
            };

            return JsonResponse.Ok(context, responseModel);
        }
    }
}
