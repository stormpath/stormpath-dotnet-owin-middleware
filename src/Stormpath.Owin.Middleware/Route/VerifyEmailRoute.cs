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
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Common;
using Stormpath.Owin.Common.ViewModel;
using Stormpath.Owin.Common.ViewModelBuilder;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class VerifyEmailRoute : AbstractRouteMiddleware
    {
        public VerifyEmailRoute(
            StormpathConfiguration configuration,
            ILogger logger,
            IClient client)
            : base(configuration, logger, client)
        {
        }

        private Task<bool> RenderForm(IOwinEnvironment context, VerifyEmailViewModel viewModel, CancellationToken cancellationToken)
        {
            var verifyEmailView = new Common.View.VerifyEmail();
            return HttpResponse.Ok(verifyEmailView, viewModel, context);
        }

        protected override async Task<bool> GetHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                var viewModelBuilder = new VerifyEmailViewModelBuilder(_configuration.Web);
                var forgotViewModel = viewModelBuilder.Build();

                return await RenderForm(context, forgotViewModel, cancellationToken);
            }

            try
            {
                await client.VerifyAccountEmailAsync(spToken, cancellationToken);

                return await HttpResponse.Redirect(context, $"{_configuration.Web.VerifyEmail.NextUri}?status=unverified");
            }
            catch (ResourceException)
            {
                var viewModelBuilder = new VerifyEmailViewModelBuilder(_configuration.Web);
                var forgotViewModel = viewModelBuilder.Build();
                forgotViewModel.InvalidSpToken = true;

                return await RenderForm(context, forgotViewModel, cancellationToken);
            }
        }

        protected override async Task<bool> PostHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var postContent = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var formData = FormContentParser.Parse(postContent);

            var email = formData.GetString("email");

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            try
            {
                await application.SendVerificationEmailAsync(email, cancellationToken);
            }
            catch (ResourceException rex) when (rex.Code == 2016)
            {
                // Code 2016 means that an account does not exist for the given email
                // address.  We don't want to leak information about the account
                // list, so allow this continue without error.
                _logger.Info($"A user tried to resend their account verification email, but failed: {rex.DeveloperMessage}");
            }
            catch (ResourceException rex)
            {
                var viewModelBuilder = new VerifyEmailViewModelBuilder(_configuration.Web);
                var verifyEmailViewModel = viewModelBuilder.Build();
                verifyEmailViewModel.Errors.Add(rex.Message);

                return await RenderForm(context, verifyEmailViewModel, cancellationToken);
            }

            return await HttpResponse.Redirect(context, $"{_configuration.Web.VerifyEmail.NextUri}?status=unverified");
        }

        protected override async Task<bool> GetJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                return await Error.Create(context, new BadRequest("sptoken parameter not provided."), cancellationToken);
            }

            await client.VerifyAccountEmailAsync(spToken, cancellationToken);
            // Errors are caught in AbstractRouteMiddleware

            return await JsonResponse.Ok(context);
        }
    }
}
