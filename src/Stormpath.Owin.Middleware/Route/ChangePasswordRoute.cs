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
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Model.Error;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class ChangePasswordRoute : AbstractRoute
    {
        public static bool ShouldBeEnabled(IntegrationConfiguration configuration)
            => configuration.Web.ChangePassword.Enabled == true;
            //|| (configuration.Web.ChangePassword.Enabled == null && configuration.Tenant.PasswordResetWorkflowEnabled);
            // TODO - will any part of this be autoconfigured?

        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                return await HttpResponse.Redirect(context, _configuration.Web.ForgotPassword.Uri);
            }

            // TODO verify password reset token
            throw new Exception("TODO");

            //try
            //{
            //    await application.VerifyPasswordResetTokenAsync(spToken, cancellationToken);

            //    var viewModelBuilder = new ChangePasswordFormViewModelBuilder(_configuration);
            //    var changePasswordViewModel = viewModelBuilder.Build();

            //    await RenderViewAsync(context, _configuration.Web.ChangePassword.View, changePasswordViewModel, cancellationToken);
            //    return true;
            //}
            //catch (Exception)
            //{
            //    return await HttpResponse.Redirect(context, _configuration.Web.ChangePassword.ErrorUri);
            //}
        }

        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);

            var body = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var model = PostBodyParser.ToModel<ChangePasswordPostModel>(body, bodyContentType, _logger);
            var formData = FormContentParser.Parse(body, _logger);

            var stateToken = formData.GetString(StringConstants.StateTokenName);
            // TODO - use Okta Client secret
            //var oktaClientSecret = "";
            throw new Exception("TODO");
            //var parsedStateToken = new StateTokenParser(oktaClientSecret, stateToken, _logger);
            //if (!parsedStateToken.Valid)
            //{
            //    var viewModelBuilder = new ChangePasswordFormViewModelBuilder(_configuration);
            //    var changePasswordViewModel = viewModelBuilder.Build();
            //    changePasswordViewModel.Errors.Add("An error occurred. Please try again.");

            //    await RenderViewAsync(context, _configuration.Web.ChangePassword.View, changePasswordViewModel, cancellationToken);
            //    return true;
            //}

            //if (!model.Password.Equals(model.ConfirmPassword, StringComparison.Ordinal))
            //{
            //    var viewModelBuilder = new ChangePasswordFormViewModelBuilder(_configuration);
            //    var changePasswordViewModel = viewModelBuilder.Build();
            //    changePasswordViewModel.Errors.Add("Passwords do not match.");

            //    await RenderViewAsync(context, _configuration.Web.ChangePassword.View, changePasswordViewModel, cancellationToken);
            //    return true;
            //}

            // todo how does the password reset flow work?
            throw new Exception("TODO");

            //var spToken = queryString.GetString("sptoken");

            //dynamic account;
            //try
            //{
            //    account = await application.VerifyPasswordResetTokenAsync(spToken, cancellationToken);
            //}
            //catch (Exception)
            //{
            //    return await HttpResponse.Redirect(context, _configuration.Web.ChangePassword.ErrorUri);
            //}

            //var preChangePasswordContext = new PreChangePasswordContext(context, account);
            //await _handlers.PreChangePasswordHandler(preChangePasswordContext, cancellationToken);

            //try
            //{
            //    await application.ResetPasswordAsync(spToken, model.Password, cancellationToken);
            //}
            //catch (ResourceException rex)
            //{
            //    var viewModelBuilder = new ChangePasswordFormViewModelBuilder(client, _configuration);
            //    var changePasswordViewModel = viewModelBuilder.Build();
            //    changePasswordViewModel.Errors.Add(rex.Message);

            //    await RenderViewAsync(context, _configuration.Web.ChangePassword.View, changePasswordViewModel, cancellationToken);
            //    return true;
            //}

            //var postChangePasswordContext = new PostChangePasswordContext(context, account);
            //await _handlers.PostChangePasswordHandler(postChangePasswordContext, cancellationToken);

            //// TODO autologin

            //return await HttpResponse.Redirect(context, _configuration.Web.ChangePassword.NextUri);
        }

        protected override async Task<bool> GetJsonAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                return await Error.Create(context, new BadRequest("sptoken parameter not provided."), cancellationToken);
            }

            // todo how does the password reset flow work?
            throw new Exception("TODO");

            //var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            //await application.VerifyPasswordResetTokenAsync(spToken, cancellationToken);
            //// Errors are caught in AbstractRouteMiddleware

            //return await JsonResponse.Ok(context);
        }

        protected override async Task<bool> PostJsonAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var model = await PostBodyParser.ToModel<ChangePasswordPostModel>(context, bodyContentType, _logger, cancellationToken);

            // todo how does the password reset flow work?
            throw new Exception("TODO");

            //var account = await application.VerifyPasswordResetTokenAsync(model.SpToken, cancellationToken);
            //// Errors are caught in AbstractRouteMiddleware

            //var preChangePasswordContext = new PreChangePasswordContext(context, account);
            //await _handlers.PreChangePasswordHandler(preChangePasswordContext, cancellationToken);

            //await application.ResetPasswordAsync(model.SpToken, model.Password, cancellationToken);

            //var postChangePasswordContext = new PostChangePasswordContext(context, account);
            //await _handlers.PostChangePasswordHandler(postChangePasswordContext, cancellationToken);

            //// TODO autologin

            //return await JsonResponse.Ok(context);
        }
    }
}
