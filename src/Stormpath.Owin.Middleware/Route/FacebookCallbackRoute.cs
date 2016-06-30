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
using Stormpath.SDK.Client;
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
                // TODO move this to a central place
                var errorUri = $"{_configuration.Web.Login.Uri}?status=social_badtoken";
                return await HttpResponse.Redirect(context, errorUri);
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            var request = client.Providers()
                .Facebook()
                .Account()
                .SetAccessToken(accessToken)
                .Build();
            var result = await application.GetAccountAsync(request, cancellationToken);
            var account = result.Account;
            // todo error handling

            // Exchange stormpath token
            // todo refactor out to helper lib

            var oauthExchangeJwt = client.NewJwtBuilder()
                .SetSubject(account.Href)
                .SetIssuedAt(DateTimeOffset.UtcNow)
                .SetExpiration(DateTimeOffset.UtcNow.AddMinutes(1)) // very short
                .SetIssuer(application.Href)
                .SetClaim("status", "AUTHENTICATED")
                .SetAudience(_configuration.Client.ApiKey.Id)
                .SignWith(_configuration.Client.ApiKey.Secret, Encoding.UTF8)
                .Build();

            var exchangeRequest = OauthRequests.NewIdSiteTokenAuthenticationRequest()
                .SetJwt(oauthExchangeJwt.ToString())
                .Build();

            var exchangeResult = await application.NewIdSiteTokenAuthenticator()
                .AuthenticateAsync(exchangeRequest, cancellationToken);
            // todo error handling

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

            // TODO move this to a central place
            var errorUri = $"{_configuration.Web.Login.Uri}?status=social_failed";
            return HttpResponse.Redirect(context, errorUri);
        }
    }
}
