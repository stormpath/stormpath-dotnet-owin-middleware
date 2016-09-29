// <copyright file="GoogleCallbackRoute.cs" company="Stormpath, Inc.">
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Provider;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class GoogleCallbackRoute : AbstractRoute
    {
        public static bool ShouldBeEnabled(IntegrationConfiguration configuration)
            => configuration.Web.Social.ContainsKey("google")
               && configuration.Providers.Any(p => p.Key.Equals("google", StringComparison.OrdinalIgnoreCase));

        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var code = queryString.GetString("code");

            if (string.IsNullOrEmpty(code))
            {
                _logger.Warn("Social code was empty", nameof(GithubCallbackRoute));
                return await HttpResponse.Redirect(context, SocialExecutor.GetErrorUri(_configuration.Web.Login));
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);
            var socialExecutor = new SocialExecutor(client, _configuration, _handlers, _logger);

            try
            {
                var providerRequest = client.Providers()
                    .Google()
                    .Account()
                    .SetCode(code)
                    .Build();

                var loginResult =
                    await socialExecutor.LoginWithProviderRequestAsync(context, providerRequest, cancellationToken);

                await socialExecutor.HandleLoginResultAsync(
                    context,
                    application,
                    loginResult,
                    cancellationToken);

                return await socialExecutor.HandleRedirectAsync(context, loginResult, cancellationToken);
            }
            catch (Exception)
            {
                return await HttpResponse.Redirect(context, SocialExecutor.GetErrorUri(_configuration.Web.Login));
            }
        }
    }
}
