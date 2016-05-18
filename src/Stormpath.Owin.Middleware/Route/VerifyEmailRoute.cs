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
using Stormpath.Owin.Abstractions.ViewModel;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class VerifyEmailRoute : AbstractRoute
    {
        public static bool ShouldBeEnabled(IntegrationConfiguration configuration)
            => configuration.Web.VerifyEmail.Enabled == true
            || (configuration.Web.VerifyEmail.Enabled == null && configuration.Tenant.EmailVerificationWorkflowEnabled);

        private async Task<bool> ResendVerification(
            string email,
            IClient client,
            Func<ResourceException, CancellationToken, Task<bool>> errorHandler,
            Func<CancellationToken, Task<bool>> successHandler,
            CancellationToken cancellationToken)
        {
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
            catch (ResourceException rex) when (errorHandler != null)
            {
                return await errorHandler(rex, cancellationToken);
            }

            return await successHandler(cancellationToken);
        }

        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                var viewModelBuilder = new VerifyEmailViewModelBuilder(_configuration.Web);
                var verifyViewModel = viewModelBuilder.Build();

                await RenderViewAsync(context, _configuration.Web.VerifyEmail.View, verifyViewModel, cancellationToken);
                return true;
            }

            try
            {
                await client.VerifyAccountEmailAsync(spToken, cancellationToken);

                return await HttpResponse.Redirect(context, $"{_configuration.Web.VerifyEmail.NextUri}?status=verified");
            }
            catch (ResourceException)
            {
                var viewModelBuilder = new VerifyEmailViewModelBuilder(_configuration.Web);
                var verifyViewModel = viewModelBuilder.Build();
                verifyViewModel.InvalidSpToken = true;

                await RenderViewAsync(context, _configuration.Web.VerifyEmail.View, verifyViewModel, cancellationToken);
                return true;
            }
        }

        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var model = await PostBodyParser.ToModel<VerifyEmailPostModel>(context, bodyContentType, _logger, cancellationToken);

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            var htmlErrorHandler = new Func<ResourceException, CancellationToken, Task<bool>>(async (rex, ct) =>
            {
                var viewModelBuilder = new VerifyEmailViewModelBuilder(_configuration.Web);
                var verifyEmailViewModel = viewModelBuilder.Build();
                verifyEmailViewModel.Errors.Add(rex.Message);

                await RenderViewAsync(context, _configuration.Web.VerifyEmail.View, verifyEmailViewModel, cancellationToken);
                return true;
            });

            var htmlSuccessHandler = new Func<CancellationToken, Task<bool>>(ct =>
            {
                return HttpResponse.Redirect(context, $"{_configuration.Web.VerifyEmail.NextUri}?status=unverified");
            });

            return await ResendVerification(
                model.Email,
                client,
                htmlErrorHandler,
                htmlSuccessHandler,
                cancellationToken);
        }

        protected override async Task<bool> GetJsonAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var spToken = queryString.GetString("sptoken");

            if (string.IsNullOrEmpty(spToken))
            {
                return await Error.Create(context, new BadRequest("sptoken parameter not provided."), cancellationToken);
            }

            await client.VerifyAccountEmailAsync(spToken, cancellationToken);
            // Errors are caught in AbstractRouteMiddleware

            return await JsonResponse.Ok(context);
        }

        protected override async Task<bool> PostJsonAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var model = await PostBodyParser.ToModel<VerifyEmailPostModel>(context, bodyContentType, _logger, cancellationToken);

            var jsonSuccessHandler = new Func<CancellationToken, Task<bool>>(ct =>
            {
                return JsonResponse.Ok(context);
            });

            return await ResendVerification(
                email: model.Email,
                client: client,
                errorHandler: null, // Errors are caught in AbstractRouteMiddleware
                successHandler: jsonSuccessHandler,
                cancellationToken: cancellationToken);
        }
    }
}
