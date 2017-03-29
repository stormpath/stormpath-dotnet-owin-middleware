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


namespace Stormpath.Owin.Middleware.Route
{
    public sealed class VerifyEmailRoute : AbstractRoute
    {
        public static bool ShouldBeEnabled(IntegrationConfiguration configuration)
            => configuration.Web.VerifyEmail.Enabled == true;
            //|| (configuration.Web.VerifyEmail.Enabled == null && configuration.Tenant.EmailVerificationWorkflowEnabled);
            // TODO - any reason this needs to be dynamic now?

        private Task<bool> ResendVerification(
            string email,
            IOwinEnvironment environment,
            Func<string, CancellationToken, Task<bool>> errorHandler,
            Func<CancellationToken, Task<bool>> successHandler,
            CancellationToken cancellationToken)
        {
            // todo how will email verification work?
            throw new Exception("TODO");

            //var preVerifyEmailContext = new PreVerifyEmailContext(environment)
            //{
            //    Email = email
            //};
            //await _handlers.PreVerifyEmailHandler(preVerifyEmailContext, cancellationToken);

            //try
            //{
            //    await application.SendVerificationEmailAsync(email, cancellationToken);
            //}
            //catch (ResourceException rex) when (rex.Code == 2016)
            //{
            //    // Code 2016 means that an account does not exist for the given email
            //    // address.  We don't want to leak information about the account
            //    // list, so allow this continue without error.
            //    _logger.LogInformation($"A user tried to resend their account verification email, but failed: {rex.DeveloperMessage}");
            //    return await successHandler(cancellationToken);
            //}
            //catch (ResourceException rex) when (errorHandler != null)
            //{
            //    return await errorHandler(rex.Message, cancellationToken);
            //}

            //return await successHandler(cancellationToken);
        }

        protected override Task<bool> GetHtmlAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            // todo how will email verification work?
            throw new Exception("TODO");

            //var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            //var spToken = queryString.GetString("sptoken");

            //if (string.IsNullOrEmpty(spToken))
            //{
            //    var viewModelBuilder = new VerifyEmailFormViewModelBuilder(_configuration);
            //    var verifyViewModel = viewModelBuilder.Build();

            //    await RenderViewAsync(context, _configuration.Web.VerifyEmail.View, verifyViewModel, cancellationToken);
            //    return true;
            //}

            //try
            //{
            //    var account = await client.VerifyAccountEmailAsync(spToken, cancellationToken);

            //    var postVerifyEmailContext = new PostVerifyEmailContext(context, account);
            //    await _handlers.PostVerifyEmailHandler(postVerifyEmailContext, cancellationToken);

            //    return await HttpResponse.Redirect(context, $"{_configuration.Web.VerifyEmail.NextUri}");
            //}
            //catch (ResourceException)
            //{
            //    var viewModelBuilder = new VerifyEmailFormViewModelBuilder(client, _configuration);
            //    var verifyViewModel = viewModelBuilder.Build();
            //    verifyViewModel.InvalidSpToken = true;

            //    await RenderViewAsync(context, _configuration.Web.VerifyEmail.View, verifyViewModel, cancellationToken);
            //    return true;
            //}
        }

        protected override Task<bool> PostHtmlAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            // todo how will email verification work?
            throw new Exception("TODO");

            //var htmlErrorHandler = new Func<string, CancellationToken, Task<bool>>(async (error, ct) =>
            //{
            //    var viewModelBuilder = new VerifyEmailFormViewModelBuilder(client, _configuration);
            //    var verifyEmailViewModel = viewModelBuilder.Build();
            //    verifyEmailViewModel.Errors.Add(error);

            //    await RenderViewAsync(context, _configuration.Web.VerifyEmail.View, verifyEmailViewModel, cancellationToken);
            //    return true;
            //});

            //var body = await context.Request.GetBodyAsStringAsync(cancellationToken);
            //var model = PostBodyParser.ToModel<VerifyEmailPostModel>(body, bodyContentType, _logger);

            //var formData = FormContentParser.Parse(body, _logger);
            //var stateToken = formData.GetString(StringConstants.StateTokenName);
            //var parsedStateToken = new StateTokenParser(client, _configuration.Client.ApiKey, stateToken, _logger);
            //if (!parsedStateToken.Valid)
            //{
            //    await htmlErrorHandler("An error occurred. Please try again.", cancellationToken);
            //    return true;
            //}

            //var htmlSuccessHandler = new Func<CancellationToken, Task<bool>>(
            //    ct => HttpResponse.Redirect(context, $"{_configuration.Web.Login.Uri}?status=unverified"));

            //return await ResendVerification(
            //    model.Email,
            //    client,
            //    context,
            //    htmlErrorHandler,
            //    htmlSuccessHandler,
            //    cancellationToken);
        }

        protected override Task<bool> GetJsonAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            // todo how will email verification work?
            throw new Exception("TODO");

            //var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            //var spToken = queryString.GetString("sptoken");

            //if (string.IsNullOrEmpty(spToken))
            //{
            //    return await Error.Create(context, new BadRequest("sptoken parameter not provided."), cancellationToken);
            //}

            //var account = await client.VerifyAccountEmailAsync(spToken, cancellationToken);
            //// Errors are caught in AbstractRouteMiddleware

            //var postVerifyEmailContext = new PostVerifyEmailContext(context, account);
            //await _handlers.PostVerifyEmailHandler(postVerifyEmailContext, cancellationToken);

            //return await JsonResponse.Ok(context);
        }

        protected override Task<bool> PostJsonAsync(IOwinEnvironment context, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            // todo how will email verification work?
            throw new Exception("TODO");

            //var model = await PostBodyParser.ToModel<VerifyEmailPostModel>(context, bodyContentType, _logger, cancellationToken);

            //var jsonSuccessHandler = new Func<CancellationToken, Task<bool>>(ct => JsonResponse.Ok(context));

            //return await ResendVerification(
            //    email: model.Email,
            //    client: client,
            //    environment: context,
            //    errorHandler: null, // Errors are caught in AbstractRouteMiddleware
            //    successHandler: jsonSuccessHandler,
            //    cancellationToken: cancellationToken);
        }
    }
}
