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
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
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
            var loginViewModel = BuildExtendedViewModel(context);

            return RenderForm(context, loginViewModel, cancellationToken);
        }

        private Task RenderForm(IOwinEnvironment context, LoginViewModelExtended viewModel, CancellationToken cancellationToken)
        {
            context.Response.Headers.SetString("Content-Type", Constants.HtmlContentType);

            var loginView = new Common.View.Login();
            return HttpResponse.Ok(loginView, viewModel, context);
        }

        private async Task<IOauthGrantAuthenticationResult> HandleLogin(IClient client, string login, string password, CancellationToken cancellationToken)
        {
            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin(login)
                .SetPassword(password)
                .Build();

            var passwordGrantAuthenticator = application.NewPasswordGrantAuthenticator();

            var grantResult = await passwordGrantAuthenticator
                .AuthenticateAsync(passwordGrantRequest, cancellationToken);

            return grantResult;
        }

        protected override async Task PostHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var requestBody = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var formData = FormContentParser.Parse(requestBody);

            var login = formData.GetString("login");
            var password = formData.GetString("password");

            bool missingLoginOrPassword = string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password);
            if (missingLoginOrPassword)
            {
                var loginViewModel = BuildExtendedViewModel(context);
                loginViewModel.FormErrors.Add("The login and password fields are required.");
                await RenderForm(context, loginViewModel, cancellationToken);
                return;
            }

            try
            {
                var grantResult = await HandleLogin(client, login, password, cancellationToken);

                Cookies.AddToResponse(context, client, grantResult, _configuration);
            }
            catch (ResourceException rex)
            {
                var loginViewModel = BuildExtendedViewModel(context);
                loginViewModel.FormErrors.Add(rex.Message);
                await RenderForm(context, loginViewModel, cancellationToken);
                return;
            }

            var nextUri = _configuration.Web.Login.NextUri;

            // todo: check ?next= parameter

            await HttpResponse.Redirect(context, nextUri);
            return;
        }

        protected override Task GetJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var loginViewModel = BuildViewModel();

            return JsonResponse.Ok(context, loginViewModel);
        }

        protected override async Task PostJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var bodyString = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var body = Serializer.Deserialize<LoginPostModel>(bodyString);
            var login = body?.Login;
            var password = body?.Password;

            bool missingLoginOrPassword = string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password);
            if (missingLoginOrPassword)
            {
                await Error.Create(context, new BadRequest("Missing login or password."), cancellationToken);
                return;
            }

            var grantResult = await HandleLogin(client, login, password, cancellationToken);
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

        private LoginViewModelExtended BuildExtendedViewModel(IOwinEnvironment context)
        {
            var result = new LoginViewModelExtended(BuildViewModel());

            result.DisplayUsernameOrEmail = _configuration.Web.Register.Form.Fields.Get("username")?.Enabled ?? false;
            result.ForgotPasswordEnabled = _configuration.Web.ForgotPassword.Enabled ?? false; // TODO handle null values here
            result.ForgotPasswordUri = _configuration.Web.ForgotPassword.Uri;
            result.RegistrationEnabled = _configuration.Web.Register.Enabled ?? false;
            result.VerifyEmailEnabled = _configuration.Web.VerifyEmail.Enabled ?? false; // TODO handle null values here
            result.VerifyEmailUri = _configuration.Web.VerifyEmail.Uri;

            var queryString = QueryStringParser.Parse(context.Request.QueryString);
            result.Status = queryString.GetString("status");

            return result;
        }
    }
}
