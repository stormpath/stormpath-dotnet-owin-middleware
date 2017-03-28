﻿// <copyright file="LinkedInCallbackRoute.cs" company="Stormpath, Inc.">
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

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class LinkedInCallbackRoute : AbstractRoute
    {
        public static bool ShouldBeEnabled(IntegrationConfiguration configuration)
            => configuration.Web.Social.ContainsKey("linkedin")
               && configuration.Providers.Any(p => p.Key.Equals("linkedin", StringComparison.OrdinalIgnoreCase));

        protected override Task<bool> GetHtmlAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            // todo how does social login work?
            throw new Exception("TODO");

            //var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);

            //var stateToken = queryString.GetString("state");
            //var parsedStateToken = new StateTokenParser(client, _configuration.Client.ApiKey, stateToken, _logger);
            //if (!parsedStateToken.Valid)
            //{
            //    _logger.LogWarning("State token was invalid", nameof(LinkedInCallbackRoute));
            //    return await HttpResponse.Redirect(context, SocialExecutor.CreateErrorUri(_configuration.Web.Login, stateToken: null));
            //}

            //if (queryString.ContainsKey("error"))
            //{
            //    _logger.LogWarning($"Error: '{queryString.GetString("error")}'", nameof(LinkedInCallbackRoute));
            //    return await HttpResponse.Redirect(context, SocialExecutor.CreateErrorUri(_configuration.Web.Login, stateToken));
            //}

            //var code = queryString.GetString("code");

            //if (string.IsNullOrEmpty(code))
            //{
            //    _logger.LogWarning("Social code was empty", nameof(GithubCallbackRoute));
            //    return await HttpResponse.Redirect(context, SocialExecutor.CreateErrorUri(_configuration.Web.Login, stateToken));
            //}

            //var accessToken = await ExchangeCodeAsync(code, cancellationToken);

            //if (string.IsNullOrEmpty(accessToken))
            //{
            //    _logger.LogWarning("Exchanged access token was null", source: nameof(LinkedInCallbackRoute));
            //    return await HttpResponse.Redirect(context, SocialExecutor.CreateErrorUri(_configuration.Web.Login, stateToken));
            //}

            //var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);
            //var socialExecutor = new SocialExecutor(client, _configuration, _handlers, _logger);

            //try
            //{
            //    var providerRequest = client.Providers()
            //        .LinkedIn()
            //        .Account()
            //        .SetAccessToken(accessToken)
            //        .Build();

            //    var loginResult =
            //        await socialExecutor.LoginWithProviderRequestAsync(context, providerRequest, cancellationToken);

            //    await socialExecutor.HandleLoginResultAsync(
            //        context,
            //        application,
            //        loginResult,
            //        cancellationToken);

            //    return await socialExecutor.HandleRedirectAsync(client, context, loginResult, parsedStateToken.Path, cancellationToken);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogWarning($"Got '{ex.Message}' during social login request", source: nameof(LinkedInCallbackRoute));
            //    return await HttpResponse.Redirect(context, SocialExecutor.CreateErrorUri(_configuration.Web.Login, stateToken));
            //}
        }

        private Task<string> ExchangeCodeAsync(
            string code,
            CancellationToken cancellationToken)
        {
            // todo how does social login work?
            throw new Exception("TODO");

            //var providerData = _configuration.Providers
            //    .First(p => p.Key.Equals("linkedin", StringComparison.OrdinalIgnoreCase))
            //    .Value;

            //var oauthCodeExchanger = new OauthCodeExchanger("https://www.linkedin.com/uas/oauth2/accessToken", _logger);
            //var accessToken = await oauthCodeExchanger.ExchangeCodeForAccessTokenAsync(
            //    code, 
            //    providerData.CallbackUri, 
            //    providerData.ClientId, 
            //    providerData.ClientSecret,
            //    cancellationToken);
            //return accessToken;
        }
    }
}
