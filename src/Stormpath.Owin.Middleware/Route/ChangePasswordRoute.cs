// <copyright file="ForgotRoute.cs" company="Stormpath, Inc.">
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
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.Owin.Middleware.ViewModelBuilder;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class ChangePasswordRoute : AbstractRoute
    {
        public static bool ShouldBeEnabled(IntegrationConfiguration configuration)
            => configuration.Web.ChangePassword.Enabled == true;

        private async Task ChangePasswordAsync(IOwinEnvironment context, ChangePasswordPostModel model, string spToken, CancellationToken cancellationToken)
        {
            var recoveryTransaction = await _oktaClient.VerifyRecoveryTokenAsync(spToken, cancellationToken);

            // We're using the workaround of storing a generated code in the "stormpath_migration_recovery_answer" profile field
            bool hasSelfServiceCode = recoveryTransaction?.Embedded?.User?.Profile?.ContainsKey("stormpath_migration_recovery_answer") ?? false;
            if (!hasSelfServiceCode)
            {
                _logger.LogWarning($"User ID '{recoveryTransaction?.Embedded?.User?.Id}' does not contain profile.stormpath_migration_recovery_answer");
                throw new InvalidOperationException("An unexpected error occurred");
            }

            var preChangePasswordContext = new PreChangePasswordContext(context, recoveryTransaction.Embedded.User);
            await _handlers.PreChangePasswordHandler(preChangePasswordContext, cancellationToken);

            // Exchange the self-service code for a blessed state token
            var selfServiceCode = recoveryTransaction.Embedded.User.Profile["stormpath_migration_recovery_answer"]?.ToString();
            await _oktaClient.AnswerRecoveryQuestionAsync(recoveryTransaction.StateToken, selfServiceCode, cancellationToken);
            recoveryTransaction = await _oktaClient.ResetPasswordAsync(recoveryTransaction.StateToken, model.Password, cancellationToken);

            var postChangePasswordContext = new PostChangePasswordContext(context, recoveryTransaction.Embedded.User);
            await _handlers.PostChangePasswordHandler(postChangePasswordContext, cancellationToken);
        }

        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                return await HttpResponse.Redirect(context, _configuration.Web.ForgotPassword.Uri);
            }

            var viewModelBuilder = new ChangePasswordFormViewModelBuilder(_configuration);
            var changePasswordViewModel = viewModelBuilder.Build();

            await RenderViewAsync(context, _configuration.Web.ChangePassword.View, changePasswordViewModel, cancellationToken);
            return true;
        }

        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);

            var body = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var model = PostBodyParser.ToModel<ChangePasswordPostModel>(body, bodyContentType, _logger);
            var formData = FormContentParser.Parse(body, _logger);

            var stateToken = formData.GetString(StringConstants.StateTokenName);
            var parsedStateToken = new StateTokenParser(_configuration.Okta.Application.Id, _configuration.OktaEnvironment.ClientSecret, stateToken, _logger);
            if (!parsedStateToken.Valid)
            {
                var viewModelBuilder = new ChangePasswordFormViewModelBuilder(_configuration);
                var changePasswordViewModel = viewModelBuilder.Build();
                changePasswordViewModel.Errors.Add("An error occurred. Please try again.");

                await RenderViewAsync(context, _configuration.Web.ChangePassword.View, changePasswordViewModel, cancellationToken);
                return true;
            }

            if (!model.Password.Equals(model.ConfirmPassword, StringComparison.Ordinal))
            {
                var viewModelBuilder = new ChangePasswordFormViewModelBuilder(_configuration);
                var changePasswordViewModel = viewModelBuilder.Build();
                changePasswordViewModel.Errors.Add("Passwords do not match.");

                await RenderViewAsync(context, _configuration.Web.ChangePassword.View, changePasswordViewModel, cancellationToken);
                return true;
            }

            var spToken = queryString.GetString("sptoken");

            try
            {
                await ChangePasswordAsync(context, model, spToken, cancellationToken);
            }
            catch (Exception ex)
            {
                var viewModelBuilder = new ChangePasswordFormViewModelBuilder(_configuration);
                var changePasswordViewModel = viewModelBuilder.Build();
                changePasswordViewModel.Errors.Add(ex.Message);

                await RenderViewAsync(context, _configuration.Web.ChangePassword.View, changePasswordViewModel, cancellationToken);
                return true;
            }

            // TODO autologin

            return await HttpResponse.Redirect(context, _configuration.Web.ChangePassword.NextUri);
        }

        protected override async Task<bool> GetJsonAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                return await Error.Create(context, new BadRequest("sptoken parameter not provided."), cancellationToken);
            }

            await _oktaClient.VerifyRecoveryTokenAsync(spToken, cancellationToken);
            // Errors are caught in AbstractRouteMiddleware

            return await JsonResponse.Ok(context);
        }

        protected override async Task<bool> PostJsonAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var model = await PostBodyParser.ToModel<ChangePasswordPostModel>(context, bodyContentType, _logger, cancellationToken);

            await ChangePasswordAsync(context, model, model.SpToken, cancellationToken);
            // Errors are caught in AbstractRouteMiddleware

            // TODO autologin

            return await JsonResponse.Ok(context);
        }
    }
}
