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
using Stormpath.Owin.Common;
using Stormpath.Owin.Common.ViewModel;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class RegisterRoute : AbstractRoute
    {
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

        private async Task<IAccount> HandleRegistration(RegisterPostModel postData, IClient client, Func<string, CancellationToken, Task> errorHandler, CancellationToken cancellationToken)
        {
            bool missingEmailOrPassword = string.IsNullOrEmpty(postData.Email) || string.IsNullOrEmpty(postData.Password);
            if (missingEmailOrPassword)
            {
                await errorHandler("Missing email or password.", cancellationToken);
                return null;
            }

            if (_configuration.Web.Register.Form.Fields["confirmPassword"].Enabled)
            {
                if (postData.Password != postData.ConfirmPassword)
                {
                    await errorHandler($"Passwords do not match.", cancellationToken);
                    return null;
                }
            }

            var registerViewModel = new RegisterViewModelBuilder(_configuration.Web.Register).Build();
            foreach (var field in registerViewModel.Form.Fields)
            {
                if (field.Required && !postData.AllNonEmptyFieldNames.Contains(field.Name, StringComparer.Ordinal))
                {
                    await errorHandler($"{field.Label} is missing.", cancellationToken);
                    return null;
                }
            }

            bool givenNameIsNotRequired =
                !_configuration.Web.Register.Form.Fields["givenName"].Required
                || !_configuration.Web.Register.Form.Fields["givenName"].Enabled;
            if (string.IsNullOrEmpty(postData.GivenName) && givenNameIsNotRequired)
            {
                postData.GivenName = "UNKNOWN";
            }

            bool surnameIsNotRequired =
                !_configuration.Web.Register.Form.Fields["surname"].Required
                || !_configuration.Web.Register.Form.Fields["surname"].Enabled;
            if (string.IsNullOrEmpty(postData.Surname) && surnameIsNotRequired)
            {
                postData.Surname = "UNKNOWN";
            }

            // Any custom fields must be defined in configuration
            var definedCustomFields = registerViewModel.Form.Fields
                .Where(f => !defaultFields.Contains(f.Name))
                .Select(f => f.Name);

            bool containsUndefinedCustomFields = postData.CustomFields.Select(x => x.Key).Except(definedCustomFields).Any();
            if (containsUndefinedCustomFields)
            {
                await errorHandler($"Unknown field '{postData.CustomFields.Select(x => x.Key).Except(definedCustomFields).First()}'.", cancellationToken);
                return null;
            }

            var newAccount = client.Instantiate<IAccount>()
                .SetEmail(postData.Email)
                .SetPassword(postData.Password)
                .SetGivenName(postData.GivenName)
                .SetSurname(postData.Surname);

            if (!string.IsNullOrEmpty(postData.Username))
            {
                newAccount.SetUsername(postData.Username);
            }

            if (!string.IsNullOrEmpty(postData.MiddleName))
            {
                newAccount.SetMiddleName(postData.MiddleName);
            }


            foreach (var item in postData.CustomFields)
            {
                newAccount.CustomData.Put(item.Key, item.Value);
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            return await application.CreateAccountAsync(newAccount, cancellationToken);
        }

        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var viewModelBuilder = new ExtendedRegisterViewModelBuilder(_configuration.Web, null);
            var registerViewModel = viewModelBuilder.Build();

            await RenderViewAsync(context, _configuration.Web.Register.View, registerViewModel, cancellationToken);
            return true;
        }

        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var bodyString = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var formData = FormContentParser.Parse(bodyString);

            var registerPostModel = new RegisterPostModel()
            {
                Email = formData.GetString("email"),
                Password = formData.GetString("password"),
                ConfirmPassword = formData.GetString("confirmPassword"),
                GivenName = formData.GetString("givenName"),
                Surname = formData.GetString("surname"),
                MiddleName = formData.GetString("middleName"),
                Username = formData.GetString("username"),
                AllNonEmptyFieldNames = formData.Where(f => !string.IsNullOrEmpty(string.Join(",", f.Value))).Select(f => f.Key).ToList(),
            };

            var providedCustomFields = new Dictionary<string, object>();
            foreach (var item in formData.Where(f => !defaultFields.Contains(f.Key)))
            {
                providedCustomFields.Add(item.Key, string.Join(",", item.Value));
            }

            registerPostModel.CustomFields = providedCustomFields;

            var htmlErrorHandler = new Func<string, CancellationToken, Task>((message, ct) =>
            {
                var viewModelBuilder = new ExtendedRegisterViewModelBuilder(_configuration.Web, formData);
                var registerViewModel = viewModelBuilder.Build();
                registerViewModel.Errors.Add(message);

                return RenderViewAsync(context, _configuration.Web.Register.View, registerViewModel, cancellationToken);
            });

            IAccount newAccount = null;
            try
            {
                newAccount = await this.HandleRegistration(registerPostModel, client, htmlErrorHandler, cancellationToken);
                if (newAccount == null)
                {
                    return true; // Some error occurred and the handler was invoked
                }
            }
            catch (ResourceException rex)
            {
                await htmlErrorHandler(rex.Message, cancellationToken);
                return true;
            }

            var nextUri = string.Empty;
            
            if (newAccount.Status == AccountStatus.Enabled)
            {
                // TODO: Autologin
                nextUri = $"{_configuration.Web.Login.Uri}?status=created";
            }
            else if (newAccount.Status == AccountStatus.Unverified)
            {
                nextUri = $"{_configuration.Web.Login.Uri}?status=unverified";
            }
            else
            {
                nextUri = _configuration.Web.Login.Uri;
            }

            return await HttpResponse.Redirect(context, nextUri);
        }

        protected override Task<bool> GetJsonAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var viewModelBuilder = new RegisterViewModelBuilder(_configuration.Web.Register);
            var registerViewModel = viewModelBuilder.Build();

            return JsonResponse.Ok(context, registerViewModel);
        }

        protected override async Task<bool> PostJsonAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var bodyString = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var bodyDictionary = Serializer.DeserializeDictionary(bodyString);

            var registerPostModel = new RegisterPostModel()
            {
                Email = bodyDictionary.Get<string>("email"),
                Password = bodyDictionary.Get<string>("password"),
                ConfirmPassword = bodyDictionary.Get<string>("confirmPassword"),
                GivenName = bodyDictionary.Get<string>("givenName"),
                Surname = bodyDictionary.Get<string>("surname"),
                MiddleName = bodyDictionary.Get<string>("middleName"),
                Username = bodyDictionary.Get<string>("username"),
                AllNonEmptyFieldNames = bodyDictionary.Where(f => !string.IsNullOrEmpty(f.Value.ToString())).Select(f => f.Key).ToList(),
            };

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

            registerPostModel.CustomFields = providedCustomFields;

            var jsonErrorHandler = new Func<string, CancellationToken, Task>((message, ct) =>
            {
                return Error.Create(context, new BadRequest(message), ct);
            });

            var newAccount = await this.HandleRegistration(registerPostModel, client, jsonErrorHandler, cancellationToken);
            if (newAccount == null)
            {
                return true; // Some error occurred and the handler was invoked
            }

            var sanitizer = new ResponseSanitizer<IAccount>();
            var responseModel = new
            {
                account = sanitizer.Sanitize(newAccount)
            };

            return await JsonResponse.Ok(context, responseModel);
        }
    }
}
