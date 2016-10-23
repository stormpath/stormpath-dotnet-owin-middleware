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
using Stormpath.SDK.Client;
using Stormpath.SDK.Oauth;
using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Error;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class Oauth2Route : AbstractRoute
    {
        protected override Task<bool> GetAsync(IOwinEnvironment context, IClient client, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
        {
            context.Response.StatusCode = 405; // Method not allowed
            return TaskConstants.CompletedTask;
        }

        protected override async Task<bool> PostAsync(IOwinEnvironment context, IClient client, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
        {
            Caching.AddDoNotCacheHeaders(context);

            var rawBodyContentType = context.Request.Headers.GetString("Content-Type");
            var bodyContentTypeDetectionResult = ContentNegotiation.DetectBodyType(rawBodyContentType);

            var isValidContentType = bodyContentTypeDetectionResult.Success && bodyContentTypeDetectionResult.ContentType == ContentType.FormUrlEncoded;

            if (!isValidContentType)
            {
                await Error.Create<OauthInvalidRequest>(context, cancellationToken);
                return true;
            }

            var requestBody = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var formData = FormContentParser.Parse(requestBody, _logger);

            var grantType = formData.GetString("grant_type");

            if (string.IsNullOrEmpty(grantType))
            {
                await Error.Create<OauthInvalidRequest>(context, cancellationToken);
                return true;
            }

            try
            {
                if (grantType.Equals("client_credentials", StringComparison.OrdinalIgnoreCase))
                {
                    await ExecuteClientCredentialsFlow(context, client, cancellationToken);
                    return true;
                }

                if (grantType.Equals("password", StringComparison.OrdinalIgnoreCase))
                {
                    var username = WebUtility.UrlDecode(formData.GetString("username"));
                    var password = WebUtility.UrlDecode(formData.GetString("password"));
                    await ExecutePasswordFlow(context, client, username, password, cancellationToken);
                    return true;
                }

                if (grantType.Equals("refresh_token", StringComparison.OrdinalIgnoreCase))
                {
                    var refreshToken = WebUtility.UrlDecode(formData.GetString("refresh_token"));
                    await ExecuteRefreshFlow(context, client, refreshToken, cancellationToken);
                    return true;
                }
            }
            catch (ResourceException rex)
            {
                // Special handling of API errors for the OAuth route
                return await Error.Create(context, new OauthError(rex.Message, rex.GetProperty("error")), cancellationToken);
            }

            return await Error.Create<OauthUnsupportedGrant>(context, cancellationToken);
        }

        private async Task<bool> ExecuteClientCredentialsFlow(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var basicHeaderParser = new BasicAuthenticationParser(context.Request.Headers.GetString("Authorization"), _logger);
            if (!basicHeaderParser.IsValid)
            {
                await Error.Create<OauthInvalidRequest>(context, cancellationToken);
                return true;
            }

            var preLoginContext = new PreLoginContext(context)
            {
                Login = basicHeaderParser.Username
            };
            await _handlers.PreLoginHandler(preLoginContext, cancellationToken).ConfigureAwait(false);

            var request = new ClientCredentialsGrantRequest
            {
                Id = basicHeaderParser.Username,
                Secret = basicHeaderParser.Password
            };

            if (preLoginContext.AccountStore != null)
            {
                request.AccountStoreHref = preLoginContext.AccountStore.Href;
            }

            var application = await client
                .GetApplicationAsync(_configuration.Application.Href, cancellationToken)
                .ConfigureAwait(false);

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
                return await Error.Create<OauthInvalidClient>(context, cancellationToken).ConfigureAwait(false);
            }

            var accessToken = await tokenResult.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            var account = await accessToken.GetAccountAsync(cancellationToken).ConfigureAwait(false);

            var postLoginContext = new PostLoginContext(context, account);
            await _handlers.PostLoginHandler(postLoginContext, cancellationToken).ConfigureAwait(false);

            var sanitizer = new GrantResultResponseSanitizer();
            return await JsonResponse.Ok(context, sanitizer.SanitizeResponseWithoutRefreshToken(tokenResult)).ConfigureAwait(false);
        }

        private async Task<bool> ExecutePasswordFlow(IOwinEnvironment context, IClient client, string username, string password, CancellationToken cancellationToken)
        {
            var preLoginContext = new PreLoginContext(context)
            {
                Login = username
            };
            await _handlers.PreLoginHandler(preLoginContext, cancellationToken);

            var passwordGrantRequestBuilder = OauthRequests.NewPasswordGrantRequest()
                .SetLogin(preLoginContext.Login)
                .SetPassword(password);

            if (preLoginContext.AccountStore != null)
            {
                passwordGrantRequestBuilder.SetAccountStore(preLoginContext.AccountStore);
            }

            var passwordGrantRequest = passwordGrantRequestBuilder.Build();

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);
            var tokenResult = await application.NewPasswordGrantAuthenticator()
                .AuthenticateAsync(passwordGrantRequest, cancellationToken);

            var accessToken = await tokenResult.GetAccessTokenAsync(cancellationToken);
            var account = await accessToken.GetAccountAsync(cancellationToken);

            var postLoginContext = new PostLoginContext(context, account);
            await _handlers.PostLoginHandler(postLoginContext, cancellationToken);

            var sanitizer = new GrantResultResponseSanitizer();
            return await JsonResponse.Ok(context, sanitizer.SanitizeResponseWithRefreshToken(tokenResult)).ConfigureAwait(false);
        }

        private async Task<bool> ExecuteRefreshFlow(IOwinEnvironment context, IClient client, string refreshToken, CancellationToken cancellationToken)
        {
            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            var refreshGrantRequest = OauthRequests.NewRefreshGrantRequest()
                .SetRefreshToken(refreshToken)
                .Build();

            var tokenResult = await application.NewRefreshGrantAuthenticator()
                .AuthenticateAsync(refreshGrantRequest, cancellationToken);

            var sanitizer = new GrantResultResponseSanitizer();
            return await JsonResponse.Ok(context, sanitizer.SanitizeResponseWithRefreshToken(tokenResult)).ConfigureAwait(false);
        }
    }
}
