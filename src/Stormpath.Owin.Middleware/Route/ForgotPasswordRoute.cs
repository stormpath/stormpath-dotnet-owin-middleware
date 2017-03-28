﻿// <copyright file="ForgotPasswordRoute.cs" company="Stormpath, Inc.">
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
using Stormpath.Owin.Abstractions.ViewModel;
using Stormpath.Owin.Middleware.Internal;


namespace Stormpath.Owin.Middleware.Route
{
    public sealed class ForgotPasswordRoute : AbstractRoute
    {
        public static bool ShouldBeEnabled(IntegrationConfiguration configuration)
            => configuration.Web.ForgotPassword.Enabled == true
                || (configuration.Web.ForgotPassword.Enabled == null && configuration.Tenant.PasswordResetWorkflowEnabled);

        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);

            var viewModelBuilder = new ForgotPasswordFormViewModelBuilder(_configuration, queryString);
            var forgotViewModel = viewModelBuilder.Build();

            await RenderViewAsync(context, _configuration.Web.ForgotPassword.View, forgotViewModel, cancellationToken);
            return true;
        }

        protected override Task<bool> PostHtmlAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            // todo how does the password reset flow work?
            throw new Exception("TODO");

            //try
            //{
            //    var body = await context.Request.GetBodyAsStringAsync(cancellationToken);
            //    var model = PostBodyParser.ToModel<ForgotPasswordPostModel>(body, bodyContentType, _logger);
            //    var formData = FormContentParser.Parse(body, _logger);

            //    var stateToken = formData.GetString(StringConstants.StateTokenName);
            //    var parsedStateToken = new StateTokenParser(client, _configuration.Client.ApiKey, stateToken, _logger);
            //    if (!parsedStateToken.Valid)
            //    {
            //        var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            //        var viewModelBuilder = new ForgotPasswordFormViewModelBuilder(client, _configuration, queryString);
            //        var viewModel = viewModelBuilder.Build();
            //        viewModel.Errors.Add("An error occurred. Please try again.");

            //        await RenderViewAsync(context, _configuration.Web.ForgotPassword.View, viewModel, cancellationToken);
            //        return true;
            //    }

            //    await application.SendPasswordResetEmailAsync(model.Email, cancellationToken);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(1002, ex, "ForgotRoute.PostHtml");
            //}

            //return await HttpResponse.Redirect(context, _configuration.Web.ForgotPassword.NextUri);
        }

        protected override Task<bool> PostJsonAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            // todo how does the password reset flow work?
            throw new Exception("TODO");

            //try
            //{
            //    var model = await PostBodyParser.ToModel<ForgotPasswordPostModel>(context, bodyContentType, _logger, cancellationToken);

            //    await application.SendPasswordResetEmailAsync(model.Email, cancellationToken);
            //}
            //catch(Exception ex)
            //{
            //    _logger.LogError(1003, ex, "ForgotRoute.PostJson");
            //}

            //return await JsonResponse.Ok(context);
        }
    }
}
