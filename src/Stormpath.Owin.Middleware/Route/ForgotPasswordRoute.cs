// <copyright file="ForgotPasswordRoute.cs" company="Stormpath, Inc.">
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
using Stormpath.Owin.Middleware.Owin;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class ForgotPasswordRoute : AbstractRouteMiddleware
    {
        private readonly static string[] SupportedMethods = { "GET", "POST" };
        private readonly static string[] SupportedContentTypes = { "text/html", "application/json" };

        public ForgotPasswordRoute(
            StormpathConfiguration configuration,
            ILogger logger,
            IClient client)
            : base(configuration, logger, client, SupportedMethods, SupportedContentTypes)
        {
        }

        private Task<bool> RenderForm(IOwinEnvironment context, ForgotPasswordViewModel viewModel, CancellationToken cancellationToken)
        {
            context.Response.Headers.SetString("Content-Type", Constants.HtmlContentType);

            var forgotView = new Common.View.Forgot();
            return HttpResponse.Ok(forgotView, viewModel, context);
        }

        protected override Task<bool> GetHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString);

            var viewModelBuilder = new ForgotPasswordViewModelBuilder(_configuration.Web, queryString);
            var forgotViewModel = viewModelBuilder.Build();

            return RenderForm(context, forgotViewModel, cancellationToken);
        }

        protected override async Task<bool> PostHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            try
            {
                var requestBody = await context.Request.GetBodyAsStringAsync(cancellationToken);
                var formData = FormContentParser.Parse(requestBody);
                var email = formData.GetString("email");

                await application.SendPasswordResetEmailAsync(email, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, source: "ForgotRoute.PostHtml");
            }

            return await HttpResponse.Redirect(context, _configuration.Web.ForgotPassword.NextUri);
        }

        protected override async Task<bool> PostJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            try
            {
                var bodyString = await context.Request.GetBodyAsStringAsync(cancellationToken);
                var body = Serializer.Deserialize<ForgotPasswordPostModel>(bodyString);
                var email = body?.Email;

                await application.SendPasswordResetEmailAsync(email, cancellationToken);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, source: "ForgotRoute.PostJson");
            }

            return await JsonResponse.Ok(context);
        }
    }
}
