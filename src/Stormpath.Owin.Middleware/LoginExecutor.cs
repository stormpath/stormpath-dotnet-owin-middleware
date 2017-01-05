// <copyright file="PostLoginExecutor.cs" company="Stormpath, Inc.">
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
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.SDK.Account;
using Stormpath.SDK.Application;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Oauth;

namespace Stormpath.Owin.Middleware
{
    internal sealed class LoginExecutor
    {
        private readonly IClient _client;
        private readonly StormpathConfiguration _configuration;
        private readonly HandlerConfiguration _handlers;
        private readonly ILogger _logger;

        private string _nextUriFromPostHandler = null;

        public LoginExecutor(
            IClient client,
            StormpathConfiguration configuration,
            HandlerConfiguration handlers,
            ILogger logger)
        {
            _client = client;
            _configuration = configuration;
            _handlers = handlers;
            _logger = logger;
        }

        public async Task<IOauthGrantAuthenticationResult> PasswordGrantAsync(
            IOwinEnvironment environment,
            IApplication application,
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

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin(preLoginHandlerContext.Login)
                .SetPassword(password);

            if (preLoginHandlerContext.AccountStore != null)
            {
                passwordGrantRequest.SetAccountStore(preLoginHandlerContext.AccountStore);
            }

            if (!string.IsNullOrEmpty(preLoginHandlerContext.OrganizationNameKey))
            {
                passwordGrantRequest.SetOrganizationNameKey(preLoginHandlerContext.OrganizationNameKey);
            }

            var passwordGrantAuthenticator = application.NewPasswordGrantAuthenticator();
            var grantResult = await passwordGrantAuthenticator
                .AuthenticateAsync(passwordGrantRequest.Build(), cancellationToken);

            return grantResult;
        }

        public async Task<IOauthGrantAuthenticationResult> ClientCredentialsGrantAsync(
            IOwinEnvironment environment,
            IApplication application,
            Func<AbstractError, CancellationToken, Task> errorHandler,
            string id,
            string secret,
            CancellationToken cancellationToken)
        {
            var preLoginHandlerContext = new PreLoginContext(environment)
            {
                Login = id
            };

            await _handlers.PreLoginHandler(preLoginHandlerContext, cancellationToken);

            if (preLoginHandlerContext.Result != null)
            {
                if (!preLoginHandlerContext.Result.Success)
                {
                    var message = string.IsNullOrEmpty(preLoginHandlerContext.Result.ErrorMessage)
                        ? "An error has occurred. Please try again."
                        : preLoginHandlerContext.Result.ErrorMessage;
                    await errorHandler(new BadRequest(message), cancellationToken);
                    return null;
                }
            }

            var request = new ClientCredentialsGrantRequest
            {
                Id = id,
                Secret = secret
            };

            if (preLoginHandlerContext.AccountStore != null)
            {
                request.AccountStoreHref = preLoginHandlerContext.AccountStore.Href;
            }

            if (!string.IsNullOrEmpty(preLoginHandlerContext.OrganizationNameKey))
            {
                request.OrganizationNameKey = preLoginHandlerContext.OrganizationNameKey;
            }

            IOauthGrantAuthenticationResult tokenResult;
            try
            {
                tokenResult = await application
                    .ExecuteOauthRequestAsync(request, cancellationToken)
                    .ConfigureAwait(false);
            }
            // Catch error 10019 (API Authentication failed)
            catch (ResourceException rex) when (rex.Code == 10019)
            {
                await errorHandler(new OauthInvalidClient(), cancellationToken);
                return null;
            }

            return tokenResult;
        }

        public async Task<IOauthGrantAuthenticationResult> TokenExchangeGrantAsync(
            IOwinEnvironment environment,
            IApplication application,
            IAccount account,
            CancellationToken cancellationToken)
        {
            var tokenExchanger = new StormpathTokenExchanger(_client, application, _configuration, _logger);
            return await tokenExchanger.ExchangeAsync(account, cancellationToken);
        }

        public async Task HandlePostLoginAsync(
            IOwinEnvironment context,
            IOauthGrantAuthenticationResult grantResult,
            CancellationToken cancellationToken)
        {
            var accessToken = await grantResult.GetAccessTokenAsync(cancellationToken);
            var account = await accessToken.GetAccountAsync(cancellationToken);

            var postLoginHandlerContext = new PostLoginContext(context, account);
            await _handlers.PostLoginHandler(postLoginHandlerContext, cancellationToken);

            // Save the custom redirect URI from the handler, if any
            _nextUriFromPostHandler = postLoginHandlerContext.Result?.RedirectUri;

            // Add Stormpath cookies
            Cookies.AddTokenCookiesToResponse(context, _client, grantResult, _configuration, _logger);
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