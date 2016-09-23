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
using Stormpath.SDK.Application;
using Stormpath.SDK.Client;
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
            string login,
            string password,
            CancellationToken cancellationToken)
        {
            var preLoginHandlerContext = new PreLoginContext(environment)
            {
                Login = login
            };

            await _handlers.PreLoginHandler(preLoginHandlerContext, cancellationToken);

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin(preLoginHandlerContext.Login)
                .SetPassword(password);

            if (preLoginHandlerContext.AccountStore != null)
            {
                passwordGrantRequest.SetAccountStore(preLoginHandlerContext.AccountStore);
            }

            var passwordGrantAuthenticator = application.NewPasswordGrantAuthenticator();
            var grantResult = await passwordGrantAuthenticator
                .AuthenticateAsync(passwordGrantRequest.Build(), cancellationToken);

            return grantResult;
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

            // Add Stormpath cookies
            Cookies.AddTokenCookiesToResponse(context, _client, grantResult, _configuration, _logger);
        }

        public Task<bool> HandleRedirectAsync(IOwinEnvironment context, string nextUri = null)
        {
            // Use the provided next URI, or default to stormpath.web.login.nextUri
            var parsedNextUri = string.IsNullOrEmpty(nextUri)
                ? new Uri(_configuration.Web.Login.NextUri, UriKind.Relative)
                : new Uri(nextUri, UriKind.RelativeOrAbsolute);

            // Ensure this is a relative URI
            var nextLocation = parsedNextUri.IsAbsoluteUri
                ? parsedNextUri.PathAndQuery
                : parsedNextUri.OriginalString;

            return HttpResponse.Redirect(context, nextLocation);
        }
    }
}