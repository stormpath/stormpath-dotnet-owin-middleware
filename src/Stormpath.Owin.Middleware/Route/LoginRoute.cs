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
using Stormpath.Owin.Middleware.ViewModelBuilder;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class LoginRoute : AbstractRoute
    {
        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);

            return await RenderLoginViewAsync(context, cancellationToken, queryString, null);
        }

        private async Task<bool> RenderLoginViewAsync(
            IOwinEnvironment context,
            CancellationToken cancellationToken,
            IDictionary<string, string[]> queryString,
            IDictionary<string, string[]> previousFormData,
            string[] errors = null)
        {
            var viewModelBuilder = new LoginFormViewModelBuilder(
                _configuration,
                ChangePasswordRoute.ShouldBeEnabled(_configuration),
                VerifyEmailRoute.ShouldBeEnabled(_configuration),
                queryString,
                previousFormData,
                errors,
                _logger);
            var loginViewModel = viewModelBuilder.Build();

            await RenderViewAsync(context, _configuration.Web.Login.View, loginViewModel, cancellationToken);
            return true;
        }

        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var body = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var model = PostBodyParser.ToModel<LoginPostModel>(body, bodyContentType, _logger);
            var formData = FormContentParser.Parse(body, _logger);

            var htmlErrorHandler = new Func<string, CancellationToken, Task>((message, ct) =>
            {
                var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
                return RenderLoginViewAsync(
                    context,
                    cancellationToken,
                    queryString,
                    formData,
                    errors: new[] { message });

            });

            var stateToken = formData.GetString(StringConstants.StateTokenName);
            var parsedStateToken = new StateTokenParser(_configuration.Application.Id, _configuration.OktaEnvironment.ClientSecret, stateToken, _logger);
            if (!parsedStateToken.Valid)
            {
                await htmlErrorHandler("An error occurred. Please try again.", cancellationToken);
                return true;
            }

            bool missingLoginOrPassword = string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Password);
            if (missingLoginOrPassword)
            {
                await htmlErrorHandler("The login and password fields are required.", cancellationToken);
                return true;
            }

            var executor = new LoginExecutor(_configuration, _handlers, _oktaClient, _logger);

            try
            {
                var (grantResult, user) = await executor.PasswordGrantAsync(
                    context,
                    htmlErrorHandler,
                    model.Login,
                    model.Password,
                    cancellationToken);

                if (grantResult == null)
                {
                    return true; // The error handler was invoked
                }

                await executor.HandlePostLoginAsync(context, grantResult, user, cancellationToken);
            }
            catch (Exception ex)
            {
                await htmlErrorHandler(ex.Message, cancellationToken);
                return true;
            }

            var nextUri = parsedStateToken.Path; // Might be null

            return await executor.HandleRedirectAsync(context, nextUri);
        }

        protected override Task<bool> GetJsonAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var viewModelBuilder = new LoginViewModelBuilder(_configuration.Web.Login);
            var loginViewModel = viewModelBuilder.Build();

            return JsonResponse.Ok(context, loginViewModel);
        }

        protected override async Task<bool> PostJsonAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var model = await PostBodyParser.ToModel<LoginPostModel>(context, bodyContentType, _logger, cancellationToken);

            var jsonErrorHandler = new Func<string, CancellationToken, Task>((message, ct)
                => Error.Create(context, new BadRequest(message), ct));

            bool missingLoginOrPassword = string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Password);
            if (missingLoginOrPassword)
            {
                await jsonErrorHandler("Missing login or password.", cancellationToken);
                return true;
            }

            var executor = new LoginExecutor(_configuration, _handlers, _oktaClient, _logger);

            var (grantResult, user) = await executor.PasswordGrantAsync(
                context,
                jsonErrorHandler,
                model.Login,
                model.Password,
                cancellationToken);

            if (grantResult == null)
            {
                return true; // The error handler was invoked
            }

            var account = await executor.HandlePostLoginAsync(context, grantResult, user, cancellationToken);

            var sanitizer = new AccountResponseSanitizer();
            var responseModel = new
            {
                account = sanitizer.Sanitize(account)
            };

            return await JsonResponse.Ok(context, responseModel);
        }
    }
}
