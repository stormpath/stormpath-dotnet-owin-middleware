// <copyright file="RegisterRoute.cs" company="Stormpath, Inc.">
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
using System.Collections.Generic;
using System.Linq;
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
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class RegisterRoute : AbstractRouteMiddleware
    {
        private readonly static string[] SupportedMethods = { "GET", "POST" };
        private readonly static string[] SupportedContentTypes = { "text/html", "application/json" };

        private static readonly string[] defaultFields =
        {
            "givenName",
            "middleName",
            "surname",
            "username",
            "email",
            "password",
            "confirmPassword",
            "customData"
        };

        public RegisterRoute(
            StormpathConfiguration configuration,
            ILogger logger,
            IClient client)
            : base(configuration, logger, client, SupportedMethods, SupportedContentTypes)
        {
        }

        private Task RenderForm(IOwinEnvironment context, ExtendedRegisterViewModel viewModel, CancellationToken cancellationToken)
        {
            context.Response.Headers.SetString("Content-Type", Constants.HtmlContentType);

            var registerView = new Common.View.Register();
            return HttpResponse.Ok(registerView, viewModel, context);
        }

        protected override Task GetHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var viewModelBuilder = new ExtendedRegisterViewModelBuilder(_configuration.Web, null);
            var registerViewModel = viewModelBuilder.Build();

            return RenderForm(context, registerViewModel, cancellationToken);
        }

        protected override Task GetJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var viewModelBuilder = new RegisterViewModelBuilder(_configuration.Web.Register);
            var registerViewModel = viewModelBuilder.Build();

            return JsonResponse.Ok(context, registerViewModel);
        }

        protected override async Task PostJson(IOwinEnvironment context, IClient scopedClient, CancellationToken cancellationToken)
        {
            var bodyString = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var bodyDictionary = Serializer.DeserializeDictionary(bodyString);

            var email = bodyDictionary.Get<string>("email");
            var password = bodyDictionary.Get<string>("password");

            bool missingEmailOrPassword = string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password);
            if (missingEmailOrPassword)
            {
                await Error.Create(context, new BadRequest("Missing email or password."), cancellationToken);
                return;
            }

            var confirmPassword = bodyDictionary.Get<string>("confirmPassword");
            if (_configuration.Web.Register.Form.Fields["confirmPassword"].Enabled)
            {
                if (password != confirmPassword)
                {
                    await Error.Create(context, new BadRequest($"Passwords do not match."), cancellationToken);
                    return;
                }
            }

            var registerViewModel = new RegisterViewModelBuilder(_configuration.Web.Register).Build();
            foreach (var field in registerViewModel.Form.Fields)
            {
                if (field.Required)
                {
                    var fieldValue = bodyDictionary.Get<string>(field.Name);
                    if (string.IsNullOrEmpty(fieldValue))
                    {
                        await Error.Create(context, new BadRequest($"Required field '{field.Name}' is missing."), cancellationToken);
                        return;
                    }
                }
            }

            var givenName = bodyDictionary.Get<string>("givenName");

            bool givenNameIsNotRequired =
                !_configuration.Web.Register.Form.Fields["givenName"].Required
                || !_configuration.Web.Register.Form.Fields["givenName"].Enabled;
            if (string.IsNullOrEmpty(givenName) && givenNameIsNotRequired)
            {
                givenName = "UNKNOWN";
            }

            var surname = bodyDictionary.Get<string>("surname");

            bool surnameIsNotRequired =
                !_configuration.Web.Register.Form.Fields["surname"].Required
                || !_configuration.Web.Register.Form.Fields["surname"].Enabled;
            if (string.IsNullOrEmpty(surname) && surnameIsNotRequired)
            {
                surname = "UNKNOWN";
            }

            var middleName = bodyDictionary.Get<string>("middleName");
            var username = bodyDictionary.Get<string>("username");

            var newAccount = scopedClient.Instantiate<IAccount>()
                .SetEmail(email)
                .SetPassword(password)
                .SetGivenName(givenName)
                .SetSurname(surname);

            if (!string.IsNullOrEmpty(username))
            {
                newAccount.SetUsername(username);
            }

            if (!string.IsNullOrEmpty(middleName))
            {
                newAccount.SetMiddleName(middleName);
            }

            // Any custom fields must be defined in configuration
            var definedCustomFields = registerViewModel.Form.Fields
                .Where(f => !defaultFields.Contains(f.Name))
                .Select(f => f.Name);

            var providedCustomFields = new Dictionary<string, object>();

            var customDataObject = bodyDictionary.Get<IDictionary<string, object>>("customData");
            if (customDataObject != null && customDataObject.Any())
            {
                foreach (var item in customDataObject)
                {
                    providedCustomFields.Add(item.Key, item.Value);
                }
            }

            foreach (var item in bodyDictionary.Where(x => !defaultFields.Contains(x.Key)))
            {
                providedCustomFields.Add(item.Key, item.Value);
            }

            bool containsUndefinedCustomFields = providedCustomFields.Select(x => x.Key).Except(definedCustomFields).Any();
            if (containsUndefinedCustomFields)
            {
                await Error.Create(context, new BadRequest($"Unknown field '{providedCustomFields.Select(x => x.Key).Except(definedCustomFields).First()}'"), cancellationToken);
                return;
            }

            foreach (var item in providedCustomFields)
            {
                newAccount.CustomData.Put(item.Key, item.Value);
            }

            var application = await scopedClient.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            await application.CreateAccountAsync(newAccount, cancellationToken);

            var sanitizer = new ResponseSanitizer<IAccount>();
            var responseModel = new
            {
                account = sanitizer.Sanitize(newAccount)
            };

            await JsonResponse.Ok(context, responseModel);
        }
    }
}
