// <copyright file="Oauth2Route.cs" company="Stormpath, Inc.">
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.Configuration.Abstractions;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Oauth;
using Stormpath.Owin.Common;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class Oauth2Route : AbstractRouteMiddleware
    {
        public Oauth2Route(
            StormpathConfiguration configuration,
            ILogger logger,
            IClient client)
            : base(configuration, logger, client)
        {
        }

        protected override async Task<bool> PostJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            if (!context.Request.Headers.GetString("Content-Type").StartsWith("application/x-www-form-urlencoded"))
            {
                await Error.Create<OauthInvalidRequest>(context, cancellationToken);
                return true;
            }

            var requestBody = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var formData = FormContentParser.Parse(requestBody);

            var grantType = formData.GetString("grant_type");
            var username = WebUtility.UrlDecode(formData.GetString("username"));
            var password = WebUtility.UrlDecode(formData.GetString("password"));

            if (string.IsNullOrEmpty(grantType))
            {
                await Error.Create<OauthInvalidRequest>(context, cancellationToken);
                return true;
            }

            if (grantType.Equals("client_credentials", StringComparison.OrdinalIgnoreCase))
            {
                await ExecuteClientCredentialsFlow(context, username, password, cancellationToken);
                return true;
            }
            else if (grantType.Equals("password", StringComparison.OrdinalIgnoreCase))
            {
                await ExecutePasswordFlow(context, client, username, password, cancellationToken);
                return true;
            }
            else
            {
                await Error.Create<OauthUnsupportedGrant>(context, cancellationToken);
                return true;
            }
        }

        private static Task ExecuteClientCredentialsFlow(IOwinEnvironment context, string username, string password, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task ExecutePasswordFlow(IOwinEnvironment context, IClient client, string username, string password, CancellationToken cancellationToken)
        {
            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin(username)
                .SetPassword(password)
                .Build();

            var tokenResult = await application.NewPasswordGrantAuthenticator()
                .AuthenticateAsync(passwordGrantRequest, cancellationToken);

            var sanitizer = new ResponseSanitizer<IOauthGrantAuthenticationResult>();
            var responseModel = sanitizer.Sanitize(tokenResult);

            await JsonResponse.Ok(context, responseModel);
        }
    }
}
