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
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.ViewModel;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Oauth;

namespace Stormpath.Owin.Middleware.Route
{
    public class LoginRoute : AbstractRoute
    {
        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString);

            var viewModelBuilder = new ExtendedLoginViewModelBuilder(
                _configuration.Web,
                ChangePasswordRoute.ShouldBeEnabled(_configuration),
                VerifyEmailRoute.ShouldBeEnabled(_configuration),
                queryString,
                null);
            var loginViewModel = viewModelBuilder.Build();

            await RenderViewAsync(context, _configuration.Web.Login.View, loginViewModel, cancellationToken);
            return true;
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

        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString);

            var body = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var model = PostBodyParser.ToModel<LoginPostModel>(body, bodyContentType);
            var formData = FormContentParser.Parse(body);

            bool missingLoginOrPassword = string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Password);
            if (missingLoginOrPassword)
            {
                var viewModelBuilder = new ExtendedLoginViewModelBuilder(
                    _configuration.Web,
                    ChangePasswordRoute.ShouldBeEnabled(_configuration),
                    VerifyEmailRoute.ShouldBeEnabled(_configuration),
                    queryString,
                    formData);
                var loginViewModel = viewModelBuilder.Build();
                loginViewModel.Errors.Add("The login and password fields are required.");

                await RenderViewAsync(context, _configuration.Web.Login.View, loginViewModel, cancellationToken);
                return true;
            }

            try
            {
                var grantResult = await HandleLogin(client, model.Login, model.Password, cancellationToken);

                Cookies.AddCookiesToResponse(context, client, grantResult, _configuration);
            }
            catch (ResourceException rex)
            {
                var viewModelBuilder = new ExtendedLoginViewModelBuilder(
                    _configuration.Web,
                    ChangePasswordRoute.ShouldBeEnabled(_configuration),
                    VerifyEmailRoute.ShouldBeEnabled(_configuration),
                    queryString,
                    formData);
                var loginViewModel = viewModelBuilder.Build();
                loginViewModel.Errors.Add(rex.Message);

                await RenderViewAsync(context, _configuration.Web.Login.View, loginViewModel, cancellationToken);
                return true;
            }

            var nextUriFromQueryString = queryString.GetString("next");

            var parsedNextUri = string.IsNullOrEmpty(nextUriFromQueryString)
                ? new Uri(_configuration.Web.Login.NextUri, UriKind.Relative)
                : new Uri(nextUriFromQueryString, UriKind.RelativeOrAbsolute);

            // Ensure this is a relative URI
            var nextLocation = parsedNextUri.IsAbsoluteUri
                ? parsedNextUri.PathAndQuery
                : parsedNextUri.OriginalString;
            
            return await HttpResponse.Redirect(context, nextLocation);
        }

        protected override Task<bool> GetJsonAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var viewModelBuilder = new LoginViewModelBuilder(_configuration.Web.Login);
            var loginViewModel = viewModelBuilder.Build();

            return JsonResponse.Ok(context, loginViewModel);
        }

        protected override async Task<bool> PostJsonAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var model = await PostBodyParser.ToModel<LoginPostModel>(context, bodyContentType, cancellationToken);

            bool missingLoginOrPassword = string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Password);
            if (missingLoginOrPassword)
            {
                return await Error.Create(context, new BadRequest("Missing login or password."), cancellationToken);
            }

            var grantResult = await HandleLogin(client, model.Login, model.Password, cancellationToken);
            // Errors will be caught up in AbstractRouteMiddleware

            Cookies.AddCookiesToResponse(context, client, grantResult, _configuration);

            var token = await grantResult.GetAccessTokenAsync(cancellationToken);
            var account = await token.GetAccountAsync(cancellationToken);

            var sanitizer = new ResponseSanitizer<IAccount>();
            var responseModel = new
            {
                account = sanitizer.Sanitize(account)
            };

            return await JsonResponse.Ok(context, responseModel);
        }
    }
}
