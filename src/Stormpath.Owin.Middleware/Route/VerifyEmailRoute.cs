// <copyright file="VerifyEmailRoute.cs" company="Stormpath, Inc.">
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
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.ViewModelBuilder;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.Owin.Middleware.Model;
using System.Linq;
using Stormpath.Owin.Middleware.Okta;
using System.Collections.Generic;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class VerifyEmailRoute : AbstractRoute
    {
        public static bool ShouldBeEnabled(IntegrationConfiguration configuration)
            => configuration.Web.VerifyEmail.Enabled == true;

        private async Task<bool> ResendVerification(
            string email,
            IOwinEnvironment environment,
            Func<string, CancellationToken, Task<bool>> errorHandler,
            Func<CancellationToken, Task<bool>> successHandler,
            CancellationToken cancellationToken)
        {
            var preVerifyEmailContext = new PreVerifyEmailContext(environment)
            {
                Email = email
            };
            await _handlers.PreVerifyEmailHandler(preVerifyEmailContext, cancellationToken);

            try
            {
                var foundUsers = await _oktaClient.FindUsersByEmailAsync(email, cancellationToken);
                if (!foundUsers.Any())
                {
                    return await successHandler(cancellationToken);
                }

                var oktaUser = foundUsers.Single();

                // Generate a new code
                var updatedProperties = new Dictionary<string, object>()
                {
                    ["emailVerificationToken"] = CodeGenerator.GetCode()
                };

                oktaUser = await _oktaClient.UpdateUserProfileAsync(oktaUser.Id, updatedProperties, cancellationToken);

                var stormpathCompatibleUser = new CompatibleOktaAccount(oktaUser);

                var sendVerificationEmailContext = new SendVerificationEmailContext(environment, stormpathCompatibleUser);
                await _handlers.SendVerificationEmailHandler(sendVerificationEmailContext, cancellationToken);
            }
            catch (Exception ex) when (errorHandler != null)
            {
                return await errorHandler(ex.Message, cancellationToken);
            }

            return await successHandler(cancellationToken);
        }

        private async Task<ICompatibleOktaAccount> VerifyAccountEmailAsync(string spToken, CancellationToken cancellationToken)
        {
            var expression = $"profile.emailVerificationToken eq \"{spToken}\"";
            var foundUsers = await _oktaClient.SearchUsersAsync(expression, cancellationToken);

            if (foundUsers.Count > 1)
            {
                throw new InvalidOperationException("An unknown error occured");
            }

            var user = foundUsers.FirstOrDefault();

            object rawToken = null;
            bool tokenExists = user?.Profile.TryGetValue("emailVerificationToken", out rawToken) ?? false;
            bool tokenMatches = rawToken?.ToString().Equals(spToken, StringComparison.Ordinal) ?? false;

            if (!tokenExists || !tokenMatches)
            {
                throw new InvalidOperationException("Token is invalid");
            }

            var updatedProperties = new Dictionary<string, object>()
            {
                ["emailVerificationToken"] = null,
                ["emailVerificationStatus"] = "VERIFIED"
            };

            await _oktaClient.ActivateUserAsync(user.Id, cancellationToken);
            user = await _oktaClient.UpdateUserProfileAsync(user.Id, updatedProperties, cancellationToken);

            return new CompatibleOktaAccount(user);
        }

        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                var viewModelBuilder = new VerifyEmailFormViewModelBuilder(_configuration);
                var verifyViewModel = viewModelBuilder.Build();

                await RenderViewAsync(context, _configuration.Web.VerifyEmail.View, verifyViewModel, cancellationToken);
                return true;
            }

            try
            {
                var account = await VerifyAccountEmailAsync(spToken, cancellationToken);

                var postVerifyEmailContext = new PostVerifyEmailContext(context, account);
                await _handlers.PostVerifyEmailHandler(postVerifyEmailContext, cancellationToken);

                return await HttpResponse.Redirect(context, $"{_configuration.Web.VerifyEmail.NextUri}");
            }
            catch (Exception)
            {
                var viewModelBuilder = new VerifyEmailFormViewModelBuilder(_configuration);
                var verifyViewModel = viewModelBuilder.Build();
                verifyViewModel.InvalidSpToken = true;

                await RenderViewAsync(context, _configuration.Web.VerifyEmail.View, verifyViewModel, cancellationToken);
                return true;
            }
        }

        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var htmlErrorHandler = new Func<string, CancellationToken, Task<bool>>(async (error, ct) =>
            {
                var viewModelBuilder = new VerifyEmailFormViewModelBuilder(_configuration);
                var verifyEmailViewModel = viewModelBuilder.Build();
                verifyEmailViewModel.Errors.Add(error);

                await RenderViewAsync(context, _configuration.Web.VerifyEmail.View, verifyEmailViewModel, cancellationToken);
                return true;
            });

            var body = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var model = PostBodyParser.ToModel<VerifyEmailPostModel>(body, bodyContentType, _logger);

            var formData = FormContentParser.Parse(body, _logger);
            var stateToken = formData.GetString(StringConstants.StateTokenName);
            var parsedStateToken = new StateTokenParser(_configuration.Application.Id, _configuration.OktaEnvironment.ClientSecret, stateToken, _logger);
            if (!parsedStateToken.Valid)
            {
                await htmlErrorHandler("An error occurred. Please try again.", cancellationToken);
                return true;
            }

            var htmlSuccessHandler = new Func<CancellationToken, Task<bool>>(
                ct => HttpResponse.Redirect(context, $"{_configuration.Web.Login.Uri}?status=unverified"));

            return await ResendVerification(
                model.Email,
                context,
                htmlErrorHandler,
                htmlSuccessHandler,
                cancellationToken);
        }

        protected override async Task<bool> GetJsonAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                return await Error.Create(context, new BadRequest("sptoken parameter not provided."), cancellationToken);
            }

            var account = await VerifyAccountEmailAsync(spToken, cancellationToken);
            // Errors are caught in AbstractRouteMiddleware

            var postVerifyEmailContext = new PostVerifyEmailContext(context, account);
            await _handlers.PostVerifyEmailHandler(postVerifyEmailContext, cancellationToken);

            return await JsonResponse.Ok(context);
        }

        protected override async Task<bool> PostJsonAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var model = await PostBodyParser.ToModel<VerifyEmailPostModel>(context, bodyContentType, _logger, cancellationToken);

            var jsonSuccessHandler = new Func<CancellationToken, Task<bool>>(ct => JsonResponse.Ok(context));

            return await ResendVerification(
                email: model.Email,
                environment: context,
                errorHandler: null, // Errors are caught in AbstractRouteMiddleware
                successHandler: jsonSuccessHandler,
                cancellationToken: cancellationToken);
        }
    }
}
