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
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Common.ViewModel;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.SDK;
using Stormpath.SDK.Account;
using Stormpath.SDK.Auth;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Oauth;

namespace Stormpath.Owin.Middleware.Route
{
    public class LoginRoute : AbstractRouteMiddleware
    {
        private readonly static string[] SupportedMethods = { "GET", "POST" };
        private readonly static string[] SupportedContentTypes = { "text/html", "application/json" };

        public LoginRoute(
            StormpathConfiguration configuration,
            ILogger logger,
            IClient client)
            : base(configuration, logger, client, SupportedMethods, SupportedContentTypes)
        {
        }

        protected override Task GetHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // todo
            context.Response.Headers.SetString("Content-Type", Constants.HtmlContentType);

            var loginViewModel = BuildExtendedViewModel();

            var loginView = new Common.View.Login();
            return HttpResponse.Ok(loginView, loginViewModel, context);
        }

        protected override Task GetJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var loginViewModel = BuildViewModel();

            return JsonResponse.Ok(context, loginViewModel);
        }

        protected override async Task PostJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var body = await context.Request.GetBodyAsAsync<LoginPostModel>(cancellationToken);
            var usernameOrEmail = body?.Login;
            var password = body?.Password;

            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                await Error.Create(context, new BadRequest("Missing login or password."), cancellationToken);
                return;
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin(usernameOrEmail)
                .SetPassword(password)
                .Build();

            var passwordGrantAuthenticator = application.NewPasswordGrantAuthenticator();

            var grantResult = await passwordGrantAuthenticator
                .AuthenticateAsync(passwordGrantRequest, cancellationToken);
            // Errors will be caught up in AbstractRouteMiddleware

            Cookies.AddToResponse(context, client, grantResult, _configuration);

            var token = await grantResult.GetAccessTokenAsync(cancellationToken);
            var account = await token.GetAccountAsync(cancellationToken);

            var sanitizer = new ResponseSanitizer<IAccount>();
            var responseModel = new
            {
                account = sanitizer.Sanitize(account)
            };

            await JsonResponse.Ok(context, responseModel);
            return;
        }

        private LoginViewModel BuildViewModel()
        {
            var result = new LoginViewModel();

            foreach (var fieldName in _configuration.Web.Login.Form.FieldOrder)
            {
                Configuration.Abstractions.Model.WebFieldConfiguration field = null;
                if (!_configuration.Web.Login.Form.Fields.TryGetValue(fieldName, out field))
                {
                    throw new Exception($"Invalid field '{fieldName}' in fieldOrder list.");
                }

                result.Form.Fields.Add(new LoginFormFieldViewModel()
                {
                    Label = field.Label,
                    Name = fieldName,
                    Placeholder = field.Placeholder,
                    Required = field.Required,
                    Type = field.Type
                });
            }

            return result;
        }

        private LoginViewModelExtended BuildExtendedViewModel()
        {
            var result = new LoginViewModelExtended(BuildViewModel());

            result.DisplayUsernameOrEmail = _configuration.Web.Register.Form.Fields.Get("username")?.Enabled ?? false;
            result.ForgotPasswordEnabled = _configuration.Web.ForgotPassword.Enabled ?? false; // TODO handle null values here
            result.ForgotPasswordUri = _configuration.Web.ForgotPassword.Uri;
            //result.FormData - set to previous result
            result.RegistrationEnabled = _configuration.Web.Register.Enabled ?? false;
            //result.Status - set to querystring param
            result.VerifyEmailEnabled = _configuration.Web.VerifyEmail.Enabled ?? false; // TODO handle null values here
            result.VerifyEmailUri = _configuration.Web.VerifyEmail.Uri;

            return result;
        }
    }
}
