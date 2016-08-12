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
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.ViewModel;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Error;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class RegisterRoute : AbstractRoute
    {
        private static readonly string[] defaultFields = Configuration.Abstractions.Default.Configuration.Web.Register.Form.Fields.Select(kvp => kvp.Key).ToArray();

        private async Task<IAccount> HandleRegistration(
            IOwinEnvironment environment,
            RegisterPostModel postData,
            IEnumerable<string> fieldNames,
            Dictionary<string, object> customFields,
            IClient client,
            Func<string, CancellationToken, Task> errorHandler,
            Func<PreRegistrationContext, CancellationToken, Task> preRegistrationHandler,
            Func<PostRegistrationContext, CancellationToken, Task> postRegistrationHandler,
            CancellationToken cancellationToken)
        {
            var viewModel = new RegisterViewModelBuilder(_configuration.Web.Register).Build();
            var suppliedFieldNames = fieldNames?.ToArray() ?? new string[] {};

            bool missingEmailOrPassword = string.IsNullOrEmpty(postData.Email) || string.IsNullOrEmpty(postData.Password);
            if (missingEmailOrPassword)
            {
                await errorHandler("Missing email or password.", cancellationToken);
                return null;
            }

            bool hasConfirmPasswordField = viewModel.Form.Fields.Where(f => f.Name == "confirmPassword").Any();
            if (hasConfirmPasswordField)
            {
                if (postData.Password != postData.ConfirmPassword)
                {
                    await errorHandler($"Passwords do not match.", cancellationToken);
                    return null;
                }
            }

            foreach (var field in viewModel.Form.Fields)
            {
                if (field.Required && !suppliedFieldNames.Contains(field.Name, StringComparer.Ordinal))
                {
                    await errorHandler($"{field.Label} is missing.", cancellationToken);
                    return null;
                }
            }

            var givenNameField = viewModel.Form.Fields.Where(f => f.Name == "givenName").SingleOrDefault();
            bool isGivenNameRequired = givenNameField?.Required ?? false;
            if (string.IsNullOrEmpty(postData.GivenName) && !isGivenNameRequired)
            {
                postData.GivenName = "UNKNOWN";
            }

            var surnameField = viewModel.Form.Fields.Where(f => f.Name == "surname").SingleOrDefault();
            bool isSurnameRequired = surnameField?.Required ?? false;
            if (string.IsNullOrEmpty(postData.Surname) && !isSurnameRequired)
            {
                postData.Surname = "UNKNOWN";
            }

            var enabledFields = viewModel.Form.Fields.Select(f => f.Name);

            var undefinedFields = suppliedFieldNames
                .Concat(customFields.Select(f => f.Key))
                .Except(enabledFields);
            if (undefinedFields.Any())
            {
                await errorHandler($"Unknown field '{undefinedFields.First()}'.", cancellationToken);
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

            foreach (var item in customFields)
            {
                newAccount.CustomData.Put(item.Key, item.Value);
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);
            var defaultAccountStore = await application.GetDefaultAccountStoreAsync(cancellationToken);

            var preRegisterHandlerContext = new PreRegistrationContext(environment, newAccount)
            {
                AccountStore = defaultAccountStore as IDirectory
            };

            await preRegistrationHandler(preRegisterHandlerContext, cancellationToken);

            IAccount createdAccount;

            if (preRegisterHandlerContext.AccountStore != null)
            {
                createdAccount = await preRegisterHandlerContext.AccountStore.CreateAccountAsync(
                    preRegisterHandlerContext.Account,
                    preRegisterHandlerContext.Options,
                    cancellationToken);
            }
            else
            {
                createdAccount = await application.CreateAccountAsync(
                    preRegisterHandlerContext.Account,
                    preRegisterHandlerContext.Options,
                    cancellationToken);
            }

            var postRegistrationContext = new PostRegistrationContext(environment, createdAccount);
            await postRegistrationHandler(postRegistrationContext, cancellationToken);

            return createdAccount;
        }

        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var viewModelBuilder = new ExtendedRegisterViewModelBuilder(_configuration.Web, null);
            var registerViewModel = viewModelBuilder.Build();

            await RenderViewAsync(context, _configuration.Web.Register.View, registerViewModel, cancellationToken);
            return true;
        }

        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var body = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var model = PostBodyParser.ToModel<RegisterPostModel>(body, bodyContentType, _logger);
            var formData = FormContentParser.Parse(body, _logger);

            var allNonEmptyFieldNames = formData.Where(f => !string.IsNullOrEmpty(string.Join(",", f.Value))).Select(f => f.Key).ToList();

            var providedCustomFields = new Dictionary<string, object>();
            foreach (var item in formData.Where(f => !defaultFields.Contains(f.Key)))
            {
                providedCustomFields.Add(item.Key, string.Join(",", item.Value));
            }

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
                newAccount = await HandleRegistration(
                    context,
                    model,
                    allNonEmptyFieldNames,
                    providedCustomFields,
                    client,
                    htmlErrorHandler,
                    _handlers.PreRegistrationHandler,
                    _handlers.PostRegistrationHandler,
                    cancellationToken);

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

        protected override async Task<bool> PostJsonAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var body = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var model = PostBodyParser.ToModel<RegisterPostModel>(body, bodyContentType, _logger);
            var formData = Serializer.DeserializeDictionary(body);

            var allNonEmptyFieldNames = formData.Where(f => !string.IsNullOrEmpty(f.Value.ToString())).Select(f => f.Key).ToList();

            var providedCustomFields = new Dictionary<string, object>();
            foreach (var item in formData.Where(x => !defaultFields.Contains(x.Key)))
            {
                providedCustomFields.Add(item.Key, item.Value);
            }

            var customDataObject = formData.Get<IDictionary<string, object>>("customData");
            if (customDataObject != null && customDataObject.Any())
            {
                foreach (var item in customDataObject)
                {
                    providedCustomFields.Add(item.Key, item.Value);
                }
            }

            var jsonErrorHandler = new Func<string, CancellationToken, Task>((message, ct) =>
            {
                return Error.Create(context, new BadRequest(message), ct);
            });

            var newAccount = await this.HandleRegistration(
                context,
                model,
                allNonEmptyFieldNames,
                providedCustomFields,
                client,
                jsonErrorHandler,
                _handlers.PreRegistrationHandler,
                _handlers.PostRegistrationHandler,
                cancellationToken);

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
