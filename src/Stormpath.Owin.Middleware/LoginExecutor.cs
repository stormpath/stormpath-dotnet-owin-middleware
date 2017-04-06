// <copyright file="LoginExecutor.cs" company="Stormpath, Inc.">
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
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Okta;
using System.IdentityModel.Tokens.Jwt;

namespace Stormpath.Owin.Middleware
{
    internal sealed class LoginExecutor
    {
        private readonly IntegrationConfiguration _configuration;
        private readonly HandlerConfiguration _handlers;
        private readonly IOktaClient _oktaClient;
        private readonly ILogger _logger;

        private string _nextUriFromPostHandler = null;

        public LoginExecutor(
            IntegrationConfiguration configuration,
            HandlerConfiguration handlers,
            IOktaClient oktaClient,
            ILogger logger)
        {
            _configuration = configuration;
            _handlers = handlers;
            _oktaClient = oktaClient;
            _logger = logger;
        }

        public async Task<GrantResult> PasswordGrantAsync(
            IOwinEnvironment environment,
            Func<string, CancellationToken, Task> errorHandler,
            string login,
            string password,
            CancellationToken cancellationToken)
        {
            var preLoginHandlerContext = new PreLoginContext(environment)
            {
                Login = login
            };

            await _handlers.PreLoginHandler(preLoginHandlerContext, cancellationToken);

            if (preLoginHandlerContext.Result != null)
            {
                if (!preLoginHandlerContext.Result.Success)
                {
                    var message = string.IsNullOrEmpty(preLoginHandlerContext.Result.ErrorMessage)
                        ? "An error has occurred. Please try again."
                        : preLoginHandlerContext.Result.ErrorMessage;
                    await errorHandler(message, cancellationToken);
                    return null;
                }
            }

            return await _oktaClient.PostPasswordGrant(
                _configuration.OktaEnvironment.AuthorizationServerId,
                _configuration.OktaEnvironment.ClientId,
                _configuration.OktaEnvironment.ClientSecret,
                preLoginHandlerContext.Login,
                password);

            // TODO verify signature
        }

        // TODO restore
        //public async Task<LoginResult> TokenExchangeGrantAsync(
        //    IOwinEnvironment environment,
        //    dynamic account,
        //    CancellationToken cancellationToken)
        //{
        //    var tokenExchanger = new StormpathTokenExchanger(_client, application, _configuration, _logger);
        //    return await tokenExchanger.ExchangeAsync(account, cancellationToken);
        //}

        public async Task<dynamic> HandlePostLoginAsync(
            IOwinEnvironment context,
            GrantResult grantResult,
            CancellationToken cancellationToken)
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(grantResult.AccessToken);
            token.Payload.TryGetValue("uid", out object rawUid);
            if (rawUid == null)
            {
                throw new Exception("Could not get user information");
            }

            var oktaUser = await _oktaClient.GetUser(rawUid.ToString());
            var stormpathCompatibleUser = new StormpathUserTransformer(_logger).OktaToStormpathUser(oktaUser);

            var postLoginHandlerContext = new PostLoginContext(context, stormpathCompatibleUser);
            await _handlers.PostLoginHandler(postLoginHandlerContext, cancellationToken);

            //Save the custom redirect URI from the handler, if any
            _nextUriFromPostHandler = postLoginHandlerContext.Result?.RedirectUri;

            // Add Stormpath cookies
            Cookies.AddTokenCookiesToResponse(context, grantResult, _configuration, _logger);

            return stormpathCompatibleUser;
        }

        public Task<bool> HandleRedirectAsync(IOwinEnvironment context, string nextUri = null, string defaultNextUri = null)
        {
            if (string.IsNullOrEmpty(defaultNextUri))
            {
                defaultNextUri = _configuration.Web.Login.NextUri;
            }

            string nextLocation;

            // If the post-login handler set a redirect URI, use that
            if (!string.IsNullOrEmpty(_nextUriFromPostHandler))
            {
                nextLocation = _nextUriFromPostHandler;
            }
            // Or, use the next URI provided by the route handler (via the state token)
            else if (!string.IsNullOrEmpty(nextUri))
            {
                var parsedNextUri = new Uri(nextUri, UriKind.RelativeOrAbsolute);

                // Ensure this (potentially user-provided) URI is relative
                nextLocation = parsedNextUri.IsAbsoluteUri
                    ? parsedNextUri.PathAndQuery
                    : parsedNextUri.OriginalString;
            }
            else
            {
                nextLocation = defaultNextUri;
            }

            return HttpResponse.Redirect(context, nextLocation);
        }
    }
}