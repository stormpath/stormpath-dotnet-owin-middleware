// <copyright file="LinkedInCallbackRoute.cs" company="Stormpath, Inc.">
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
    public class LinkedInCallbackRoute : AbstractRoute
    {
        public static bool ShouldBeEnabled(IntegrationConfiguration configuration)
            => configuration.Web.Social.ContainsKey("linkedin")
               && configuration.Providers.Any(p => p.Key.Equals("linkedin", StringComparison.OrdinalIgnoreCase));

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

            var providerData = _configuration.Providers
                .First(p => p.Key.Equals("linkedin", StringComparison.OrdinalIgnoreCase))
                .Value;

            if (!Csrf.ConsumeOauthStateToken(context, _logger))
            {
                _logger.Info("A user attempted to log in via LinkedIn OAuth with an invalid state token.", nameof(LinkedInCallbackRoute));
                return await HttpResponse.Redirect(context, GetErrorUri());
            }

            var cookieParser = CookieParser.FromRequest(context, _logger);
            var oauthStateToken = cookieParser?.Get(Csrf.OauthStateTokenCookieName);

            var oauthCodeExchanger = new OauthCodeExchanger("https://www.linkedin.com/uas/oauth2/accessToken", _logger);
            var accessToken = await oauthCodeExchanger.ExchangeCodeForAccessTokenAsync(
                code, providerData.CallbackUri, providerData.ClientId, providerData.ClientSecret, oauthStateToken, cancellationToken);

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.Warn("Exchanged access token was null", source: nameof(LinkedInCallbackRoute));
                return await HttpResponse.Redirect(context, GetErrorUri());
            }

            IAccount account;
            var isNewAccount = false;
            try
            {
                var request = client.Providers()
                    .LinkedIn()
                    .Account()
                    .SetAccessToken(accessToken)
                    .Build();
                var result = await application.GetAccountAsync(request, cancellationToken);
                account = result.Account;
                isNewAccount = result.IsNewAccount;
            }
            catch (ResourceException rex)
            {
                _logger.Warn(rex, source: nameof(LinkedInCallbackRoute));
                account = null;
            }

            if (account == null)
            {
                return await HttpResponse.Redirect(context, GetErrorUri());
            }

            var tokenExchanger = new StormpathTokenExchanger(client, application, _configuration, _logger);
            var exchangeResult = await tokenExchanger.ExchangeAsync(account, cancellationToken);

            if (exchangeResult == null)
            {
                return await HttpResponse.Redirect(context, GetErrorUri());
            }

            if (isNewAccount)
            {
                var postRegistrationContext = new PostRegistrationContext(context, account);
                await _handlers.PostRegistrationHandler(postRegistrationContext, cancellationToken);
            }

            var postLoginContext = new PostLoginContext(context, account);
            await _handlers.PostLoginHandler(postLoginContext, cancellationToken);

            var nextUri = isNewAccount
                ? _configuration.Web.Register.NextUri
                : _configuration.Web.Login.NextUri;

            Cookies.AddTokenCookiesToResponse(context, client, exchangeResult, _configuration, _logger);
            return await HttpResponse.Redirect(context, nextUri);
        }

        protected override Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);

            if (queryString.ContainsKey("error"))
            {
                _logger.Warn($"Error: '{queryString.GetString("error")}'", nameof(LinkedInCallbackRoute));
                return HttpResponse.Redirect(context, GetErrorUri());
            }

            if (!queryString.ContainsKey("code"))
            {
                _logger.Warn("Missing code parameter", nameof(LinkedInCallbackRoute));
                return HttpResponse.Redirect(context, GetErrorUri());
            }

            return LoginWithAccessCode(queryString.GetString("code"), context, client, cancellationToken);
        }

        private string GetErrorUri()
        {
            return $"{_configuration.Web.Login.Uri}?status=social_failed";
        }
    }
}
