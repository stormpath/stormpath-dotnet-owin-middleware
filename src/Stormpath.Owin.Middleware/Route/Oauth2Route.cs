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
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class Oauth2Route : AbstractRoute
    {
        protected override Task<bool> GetAsync(IOwinEnvironment context, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
        {
            context.Response.StatusCode = 405; // Method not allowed
            return TaskConstants.CompletedTask;
        }

        protected override async Task<bool> PostAsync(IOwinEnvironment context, ContentNegotiationResult contentNegotiationResult, CancellationToken cancellationToken)
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
                if (grantType.Equals("client_credentials", StringComparison.OrdinalIgnoreCase)
                    && _configuration.Web.Oauth2.Client_Credentials.Enabled)
                {
                    await ExecuteClientCredentialsFlow(context, _oktaClient, cancellationToken);
                    return true;
                }

                if (grantType.Equals("password", StringComparison.OrdinalIgnoreCase)
                    && _configuration.Web.Oauth2.Password.Enabled)
                {
                    var username = WebUtility.UrlDecode(formData.GetString("username"));
                    var password = WebUtility.UrlDecode(formData.GetString("password"));
                    await ExecutePasswordFlow(context, username, password, cancellationToken);
                    return true;
                }

                if (grantType.Equals("refresh_token", StringComparison.OrdinalIgnoreCase)
                    && _configuration.Web.Oauth2.Password.Enabled)
                {
                    var refreshToken = WebUtility.UrlDecode(formData.GetString("refresh_token"));
                    await ExecuteRefreshFlow(context, refreshToken, cancellationToken);
                    return true;
                }
            }
            // Special handling of API errors for the OAuth route
            catch (OktaException oex)
            {
                string message = oex.Message;
                oex.Body.TryGetValue("error_description", out var rawMessage);
                if (!string.IsNullOrEmpty(rawMessage.ToString())) message = rawMessage.ToString();

                string error = "invalid_grant";
                oex.Body.TryGetValue("error", out var rawError);
                if (!string.IsNullOrEmpty(rawError.ToString())) error = rawError.ToString();

                return await Error.Create(context, new OauthError(message, error), cancellationToken);
            }
            catch (Exception ex)
            {
                return await Error.Create(context, new OauthError(ex.Message, "invalid_grant"), cancellationToken);
            }

            return await Error.Create<OauthUnsupportedGrant>(context, cancellationToken);
        }

        private async Task<bool> ExecutePasswordFlow(IOwinEnvironment context, string username, string password, CancellationToken cancellationToken)
        {
            var executor = new LoginExecutor(_configuration, _handlers, _oktaClient, _logger);

            var jsonErrorHandler = new Func<string, CancellationToken, Task>((message, ct)
                => Error.Create(context, new BadRequest(message), ct));

            var (grantResult, user) = await executor.PasswordGrantAsync(
                context,
                jsonErrorHandler,
                username,
                password,
                cancellationToken);

            await executor.HandlePostLoginAsync(context, grantResult, user, cancellationToken);

            var sanitizer = new GrantResultResponseSanitizer();
            return await JsonResponse.Ok(context, sanitizer.SanitizeResponseWithRefreshToken(grantResult)).ConfigureAwait(false);
        }

        private async Task<bool> ExecuteRefreshFlow(IOwinEnvironment context, string refreshToken, CancellationToken cancellationToken)
        {
            var grantResult = await _oktaClient.PostRefreshGrantAsync(
                _configuration.OktaEnvironment.AuthorizationServerId,
                _configuration.OktaEnvironment.ClientId,
                _configuration.OktaEnvironment.ClientSecret,
                refreshToken,
                context.CancellationToken);

            var sanitizer = new GrantResultResponseSanitizer();
            return await JsonResponse.Ok(context, sanitizer.SanitizeResponseWithRefreshToken(grantResult)).ConfigureAwait(false);
        }

        private async Task<bool> ExecuteClientCredentialsFlow(IOwinEnvironment context, IOktaClient oktaClient, CancellationToken cancellationToken)
        {
            var jsonErrorHandler = new Func<AbstractError, CancellationToken, Task>((err, ct)
                => Error.Create(context, err, ct));

            var basicHeaderParser = new BasicAuthenticationParser(context.Request.Headers.GetString("Authorization"), _logger);
            if (!basicHeaderParser.IsValid)
            {
                await jsonErrorHandler(new OauthInvalidRequest(), cancellationToken);
                return true;
            }

            var apiKey = await oktaClient.GetApiKeyAsync(basicHeaderParser.Username, cancellationToken);

            var validKey =
                apiKey != null &&
                apiKey.Status.Equals("enabled", StringComparison.OrdinalIgnoreCase) &&
                ConstantTimeComparer.Equals(apiKey.Secret, basicHeaderParser.Password);

            if (!validKey)
            {
                await jsonErrorHandler(new OauthInvalidClient(), cancellationToken);
                return true;
            }

            var validAccount =
                apiKey.User != null &&
                apiKey.User.Status.Equals("active", StringComparison.OrdinalIgnoreCase);

            if (!validAccount)
            {
                await jsonErrorHandler(new OauthInvalidClient(), cancellationToken);
                return true;
            }

            var executor = new LoginExecutor(_configuration, _handlers, _oktaClient, _logger);

            var tokenResult = await executor.ClientCredentialsGrantAsync(
                context,
                jsonErrorHandler,
                basicHeaderParser.Username,
                apiKey.User.Id,
                cancellationToken);

            if (tokenResult == null)
            {
                return true; // Some error occurred and the handler was invoked
            }
            
            await executor.HandlePostLoginAsync(context, tokenResult, apiKey.User, cancellationToken);

            var sanitizer = new GrantResultResponseSanitizer();
            return await JsonResponse.Ok(context, sanitizer.SanitizeResponseWithoutRefreshToken(tokenResult)).ConfigureAwait(false);
        }
    }
}
