// <copyright file="LoginRoute.cs" company="Stormpath, Inc.">
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
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.ViewModel;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Provider;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class LoginRoute : AbstractRoute
    {
        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);

            return await RenderLoginViewAsync(client, context, cancellationToken, queryString, null);
        }

        private async Task<bool> RenderLoginViewAsync(
            IClient client,
            IOwinEnvironment context,
            CancellationToken cancellationToken,
            IDictionary<string, string[]> queryString,
            IDictionary<string, string[]> previousFormData,
            string[] errors = null)
        {
            var viewModelBuilder = new ExtendedLoginViewModelBuilder(
                client,
                _configuration,
                ChangePasswordRoute.ShouldBeEnabled(_configuration),
                VerifyEmailRoute.ShouldBeEnabled(_configuration),
                queryString,
                previousFormData,
                errors);
            var loginViewModel = viewModelBuilder.Build();

            // TODO restore or remove
            //Cookies.AddTempCookieToResponse(
            //    context,
            //    Csrf.OauthStateTokenCookieName,
            //    loginViewModel.OauthStateToken,
            //    TimeSpan.FromMinutes(5),
            //    _logger);

            await RenderViewAsync(context, _configuration.Web.Login.View, loginViewModel, cancellationToken);
            return true;
        }

        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);

            var body = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var model = PostBodyParser.ToModel<LoginPostModel>(body, bodyContentType, _logger);
            var formData = FormContentParser.Parse(body, _logger);

            var stateToken = formData.GetString(ExtendedLoginViewModel.DefaultStateTokenName);
            var parsedStateToken = new StateTokenParser(client, _configuration.Client.ApiKey, stateToken, _logger);
            if (!parsedStateToken.Valid)
            {
                return await RenderLoginViewAsync(
                    client,
                    context,
                    cancellationToken,
                    queryString,
                    formData,
                    errors: new[] { "An error occurred. Please try again." });
            }

            bool missingLoginOrPassword = string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Password);
            if (missingLoginOrPassword)
            {
                return await RenderLoginViewAsync(
                    client,
                    context,
                    cancellationToken,
                    queryString,
                    formData,
                    errors: new[] { "The login and password fields are required." });
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);
            var executor = new LoginExecutor(client, _configuration, _handlers, _logger);

            try
            {
                var grantResult = await executor.PasswordGrantAsync(context, application, model.Login, model.Password, cancellationToken);

                await executor.HandlePostLoginAsync(context, grantResult, cancellationToken);
            }
            catch (ResourceException rex)
            {
                return await RenderLoginViewAsync(
                    client,
                    context,
                    cancellationToken,
                    queryString,
                    formData,
                    errors: new[] { rex.Message });
            }

            var nextUri = parsedStateToken.Path; // Might be null

            return await executor.HandleRedirectAsync(context, nextUri);
        }

        protected override Task<bool> GetJsonAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var viewModelBuilder = new LoginViewModelBuilder(_configuration.Web.Login, _configuration.Providers);
            var loginViewModel = viewModelBuilder.Build();

            return JsonResponse.Ok(context, loginViewModel);
        }

        protected override async Task<bool> PostJsonAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var model = await PostBodyParser.ToModel<LoginPostModel>(context, bodyContentType, _logger, cancellationToken);

            if (model.ProviderData != null)
            {
                return await HandleSocialLogin(context, client, model, cancellationToken);
            }

            bool missingLoginOrPassword = string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Password);
            if (missingLoginOrPassword)
            {
                return await Error.Create(context, new BadRequest("Missing login or password."), cancellationToken);
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);
            var executor = new LoginExecutor(client, _configuration, _handlers, _logger);

            var grantResult =
                await executor.PasswordGrantAsync(context, application, model.Login, model.Password, cancellationToken);
            // Errors will be caught up in AbstractRouteMiddleware

            await executor.HandlePostLoginAsync(context, grantResult, cancellationToken);

            var token = await grantResult.GetAccessTokenAsync(cancellationToken);
            var account = await token.GetAccountAsync(cancellationToken);

            var sanitizer = new ResponseSanitizer<IAccount>();
            var responseModel = new
            {
                account = sanitizer.Sanitize(account)
            };

            return await JsonResponse.Ok(context, responseModel);
        }

        private async Task<bool> HandleSocialLogin(IOwinEnvironment context, IClient client, LoginPostModel model,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(model.ProviderData.ProviderId))
            {
                return await Error.Create(context, new BadRequest("No provider specified"), cancellationToken);
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);
            var socialExecutor = new SocialExecutor(client, _configuration, _handlers, _logger);

            try
            {
                IProviderAccountRequest providerRequest;

                switch (model.ProviderData.ProviderId)
                {
                    case "facebook":
                    {
                        providerRequest = client.Providers()
                            .Facebook()
                            .Account()
                            .SetAccessToken(model.ProviderData.AccessToken)
                            .Build();
                        break;
                    }
                    case "google":
                    {
                        providerRequest = client.Providers()
                            .Google()
                            .Account()
                            .SetCode(model.ProviderData.Code)
                            .Build();
                        break;
                    }
                    case "github":
                    {
                        providerRequest = client.Providers()
                            .Github()
                            .Account()
                            .SetAccessToken(model.ProviderData.AccessToken)
                            .Build();
                        break;
                    }
                    case "linkedin":
                    {
                        providerRequest = client.Providers()
                            .LinkedIn()
                            .Account()
                            .SetAccessToken(model.ProviderData.AccessToken)
                            .Build();
                        break;
                    }
                    default:
                        providerRequest = null;
                        break;
                }

                if (providerRequest == null)
                {
                    return await Error.Create(context,
                        new BadRequest($"Unknown provider '{model.ProviderData.ProviderId}'"), cancellationToken);
                }

                var loginResult =
                    await socialExecutor.LoginWithProviderRequestAsync(context, providerRequest, cancellationToken);

                await socialExecutor.HandleLoginResultAsync(
                    context,
                    application,
                    loginResult,
                    cancellationToken);

                var sanitizer = new ResponseSanitizer<IAccount>();
                var responseModel = new
                {
                    account = sanitizer.Sanitize(loginResult.Account)
                };

                return await JsonResponse.Ok(context, responseModel);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, source: nameof(HandleSocialLogin));
                return await Error.Create(context, new BadRequest("An error occurred while processing the login"), cancellationToken);
            }
        }
    }
}
