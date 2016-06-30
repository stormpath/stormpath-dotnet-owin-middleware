// <copyright file="FacebookCallbackRoute.cs" company="Stormpath, Inc.">
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Oauth;
using Stormpath.SDK.Provider;

namespace Stormpath.Owin.Middleware.Route
{
    public class FacebookCallbackRoute : AbstractRoute
    {
        public static bool ShouldBeEnabled(IntegrationConfiguration configuration)
            => configuration.Web.Social.ContainsKey("facebook")
               && configuration.Providers.Any(p => p.Key.Equals("facebook", StringComparison.OrdinalIgnoreCase));

        private async Task<bool> LoginWithAccessToken(
            string accessToken,
            IOwinEnvironment context,
            IClient client,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return await HttpResponse.Redirect(context, GetErrorUri());
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            IAccount account;
            try
            {
                var request = client.Providers()
                    .Facebook()
                    .Account()
                    .SetAccessToken(accessToken)
                    .Build();
                var result = await application.GetAccountAsync(request, cancellationToken);
                account = result.Account;
            }
            catch (ResourceException rex)
            {
                _logger.Warn(rex, source: "FacebookCallbackRoute");
                account = null;
            }

            if (account == null)
            {
                return await HttpResponse.Redirect(context, GetErrorUri());
            }

            var tokenExchanger = new StormpathTokenExchanger(client, application, _configuration, _logger);
            var exchangeResult = await tokenExchanger.Exchange(accessToken, account, cancellationToken);

            if (exchangeResult == null)
            {
                return await HttpResponse.Redirect(context, GetErrorUri());
            }

            Cookies.AddCookiesToResponse(context, client, exchangeResult, _configuration, _logger);
            return await HttpResponse.Redirect(context, _configuration.Web.Login.NextUri);
        }

        protected override Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);

            if (queryString.ContainsKey("access_token"))
            {
                return LoginWithAccessToken(queryString.GetString("access_token"), context, client, cancellationToken);
            }

            return HttpResponse.Redirect(context, GetErrorUri());
        }

        private string GetErrorUri()
        {
            return $"{_configuration.Web.Login.Uri}?status=social_failed";
        }
    }
}
