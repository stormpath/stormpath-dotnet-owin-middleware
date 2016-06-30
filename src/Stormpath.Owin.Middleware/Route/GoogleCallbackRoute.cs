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
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Provider;

namespace Stormpath.Owin.Middleware.Route
{
    public class GoogleCallbackRoute : AbstractRoute
    {
        public static bool ShouldBeEnabled(IntegrationConfiguration configuration)
            => configuration.Web.Social.ContainsKey("google")
               && configuration.Providers.Any(p => p.Key.Equals("google", StringComparison.OrdinalIgnoreCase));

        private async Task<bool> LoginWithAccessCode(
            string code,
            IOwinEnvironment context,
            IClient client,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(code))
            {
                return await HttpResponse.Redirect(context, GetErrorUri());
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            IAccount account;
            var isNewAccount = false;
            try
            {
                var request = client.Providers()
                    .Google()
                    .Account()
                    .SetCode(code)
                    .Build();
                var result = await application.GetAccountAsync(request, cancellationToken);
                account = result.Account;
                isNewAccount = result.IsNewAccount;
            }
            catch (ResourceException rex)
            {
                _logger.Warn(rex, source: nameof(GoogleCallbackRoute));
                account = null;
            }

            if (account == null)
            {
                return await HttpResponse.Redirect(context, GetErrorUri());
            }

            var tokenExchanger = new StormpathTokenExchanger(client, application, _configuration, _logger);
            var exchangeResult = await tokenExchanger.Exchange(account, cancellationToken);

            if (exchangeResult == null)
            {
                return await HttpResponse.Redirect(context, GetErrorUri());
            }

            var nextUri = isNewAccount
                ? _configuration.Web.Register.NextUri
                : _configuration.Web.Login.NextUri;

            Cookies.AddCookiesToResponse(context, client, exchangeResult, _configuration, _logger);
            return await HttpResponse.Redirect(context, nextUri);
        }

        protected override Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);

            if (queryString.ContainsKey("code"))
            {
                return LoginWithAccessCode(queryString.GetString("code"), context, client, cancellationToken);
            }

            return HttpResponse.Redirect(context, GetErrorUri());
        }

        private string GetErrorUri()
        {
            return $"{_configuration.Web.Login.Uri}?status=social_failed";
        }
    }
}
