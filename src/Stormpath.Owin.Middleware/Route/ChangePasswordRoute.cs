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
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Common;
using Stormpath.Owin.Common.ViewModel;
using Stormpath.Owin.Common.ViewModelBuilder;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class ChangePasswordRoute : AbstractRouteMiddleware
    {
        public ChangePasswordRoute(
            StormpathConfiguration configuration,
            ILogger logger,
            IClient client)
            : base(configuration, logger, client)
        {
        }

        private Task<bool> RenderForm(IOwinEnvironment context, ChangePasswordViewModel viewModel, CancellationToken cancellationToken)
        {
            var loginView = new Common.View.ChangePassword();
            return HttpResponse.Ok(loginView, viewModel, context);
        }

        protected override async Task<bool> GetHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                return await HttpResponse.Redirect(context, _configuration.Web.ForgotPassword.Uri);
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href);

            try
            {
                await application.VerifyPasswordResetTokenAsync(spToken, cancellationToken);

                var viewModelBuilder = new ChangePasswordViewModelBuilder(_configuration.Web, queryString);
                var changePasswordViewModel = viewModelBuilder.Build();

                return await RenderForm(context, changePasswordViewModel, cancellationToken);
            }
            catch (ResourceException)
            {
                return await HttpResponse.Redirect(context, _configuration.Web.ChangePassword.ErrorUri);
            }
        }

        protected override async Task<bool> PostHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString);

            var postContent = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var formData = FormContentParser.Parse(postContent);

            var password = formData.GetString("password");
            var passwordAgain = formData.GetString("passwordAgain");
            if (!password.Equals(passwordAgain, StringComparison.Ordinal))
            {
                var viewModelBuilder = new ChangePasswordViewModelBuilder(_configuration.Web, queryString);
                var changePasswordViewModel = viewModelBuilder.Build();
                changePasswordViewModel.Errors.Add("Passwords do not match.");

                return await RenderForm(context, changePasswordViewModel, cancellationToken);
            }

            var spToken = queryString.GetString("sptoken");

            var application = await client.GetApplicationAsync(_configuration.Application.Href);

            try
            {
                await application.VerifyPasswordResetTokenAsync(spToken, cancellationToken);
            }
            catch (ResourceException)
            {
                return await HttpResponse.Redirect(context, _configuration.Web.ChangePassword.ErrorUri);
            }

            try
            {
                await application.ResetPasswordAsync(spToken, password, cancellationToken);
            }
            catch (ResourceException rex)
            {
                var viewModelBuilder = new ChangePasswordViewModelBuilder(_configuration.Web, queryString);
                var changePasswordViewModel = viewModelBuilder.Build();
                changePasswordViewModel.Errors.Add(rex.Message);

                return await RenderForm(context, changePasswordViewModel, cancellationToken);
            }

            // TODO autologin

            return await HttpResponse.Redirect(context, _configuration.Web.ChangePassword.NextUri);
        }

        protected override async Task<bool> GetJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                return await Error.Create(context, new BadRequest("sptoken parameter not provided."), cancellationToken);
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href);

            try
            {
                await application.VerifyPasswordResetTokenAsync(spToken, cancellationToken);

                return await JsonResponse.Ok(context);
            }
            catch (ResourceException rex)
            {
                return await Error.CreateFromApiError(context, rex, cancellationToken);
            }
        }

        protected override async Task<bool> PostJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString);

            var bodyString = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var body = Serializer.Deserialize<ChangePasswordPostModel>(bodyString);

            var password = body?.Password;
            var spToken = body?.SpToken;

            var application = await client.GetApplicationAsync(_configuration.Application.Href);

            try
            {
                await application.VerifyPasswordResetTokenAsync(spToken, cancellationToken);
            }
            catch (ResourceException rex)
            {
                return await Error.CreateFromApiError(context, rex, cancellationToken);
            }

            try
            {
                await application.ResetPasswordAsync(spToken, password, cancellationToken);
            }
            catch (ResourceException rex)
            {
                return await Error.CreateFromApiError(context, rex, cancellationToken);
            }

            // TODO autologin

            return await JsonResponse.Ok(context);
        }
    }
}
