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
        private static readonly string[] DefaultFields = Configuration.Abstractions.Default.Configuration.Web.Register.Form.Fields.Select(kvp => kvp.Key).ToArray();

        private static void TryAdd<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
        }

        private async Task<IAccount> InstantiateLocalAccount(
            IOwinEnvironment environment,
            RegisterPostModel postData,
            IEnumerable<string> fieldNames,
            Dictionary<string, string> customFields,
            IClient client,
            Func<string, CancellationToken, Task> errorHandler,
            CancellationToken cancellationToken)
        {
            var viewModel = new RegisterViewModelBuilder(_configuration.Web.Register).Build();
            var suppliedFieldNames = fieldNames?.ToArray() ?? new string[] {};

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
                    await errorHandler($"{field.Label} is required.", cancellationToken);
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

            return newAccount;
        }

        protected override async Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var viewModelBuilder = new RegisterFormViewModelBuilder(client, _configuration, queryString, null, _logger);
            var registerViewModel = viewModelBuilder.Build();

            await RenderViewAsync(context, _configuration.Web.Register.View, registerViewModel, cancellationToken);
            return true;
        }

        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var body = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var model = PostBodyParser.ToModel<RegisterPostModel>(body, bodyContentType, _logger);
            var formData = FormContentParser.Parse(body, _logger);

            var htmlErrorHandler = new Func<string, CancellationToken, Task>((message, ct) =>
            {
                var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
                var viewModelBuilder = new RegisterFormViewModelBuilder(client, _configuration, queryString, formData, _logger);
                var registerViewModel = viewModelBuilder.Build();
                registerViewModel.Errors.Add(message);

                return RenderViewAsync(context, _configuration.Web.Register.View, registerViewModel, cancellationToken);
            });

            var stateToken = formData.GetString(StringConstants.StateTokenName);
            var parsedStateToken = new StateTokenParser(client, _configuration.Client.ApiKey, stateToken, _logger);
            if (!parsedStateToken.Valid)
            {
                await htmlErrorHandler("An error occurred. Please try again.", cancellationToken);
                return true;
            }

            var allNonEmptyFieldNames = formData
                .Where(f => !string.IsNullOrEmpty(string.Join(",", f.Value)))
                .Select(f => f.Key)
                .Except(new []{ StringConstants.StateTokenName })
                .ToList();

            var providedCustomFields = new Dictionary<string, string>();
            var nonCustomFields = DefaultFields.Concat(new[] {StringConstants.StateTokenName}).ToArray();
            foreach (var item in formData.Where(f => !nonCustomFields.Contains(f.Key)))
            {
                providedCustomFields.Add(item.Key, string.Join(",", item.Value));
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);
            var executor = new RegisterExecutor(client, _configuration, _handlers, _logger);

            try
            {
                var newAccount = await InstantiateLocalAccount(
                    context,
                    model,
                    allNonEmptyFieldNames,
                    providedCustomFields,
                    client,
                    htmlErrorHandler,
                    cancellationToken);
                if (newAccount == null)
                {
                    return true; // Some error occurred and the handler was invoked
                }

                var formDataForHandler = formData
                    .ToDictionary(kv => kv.Key, kv => string.Join(",", kv.Value));

                var createdAccount = await executor.HandleRegistrationAsync(
                    context,
                    application,
                    formDataForHandler,
                    newAccount,
                    htmlErrorHandler,
                    cancellationToken);
                if (createdAccount == null)
                {
                    return true; // Some error occurred and the handler was invoked
                }

                await executor.HandlePostRegistrationAsync(context, createdAccount, cancellationToken);

                return await executor.HandleRedirectAsync(
                    context,
                    application,
                    createdAccount,
                    model,
                    htmlErrorHandler,
                    stateToken,
                    cancellationToken);
            }
            catch (ResourceException rex)
            {
                await htmlErrorHandler(rex.Message, cancellationToken);
                return true;
            }
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

            var sanitizedFormData = new Dictionary<string, string>();
            var customFields = new Dictionary<string, string>();

            // Look for a root object called "customData"
            var customDataObject = formData.Get<IDictionary<string, object>>("customData");
            if (customDataObject != null && customDataObject.Any())
            {
                foreach (var field in customDataObject)
                {
                    TryAdd(formData, field.Key, field.Value);
                }
            }

            foreach (var field in formData)
            {
                // The key "customData" is a special case, see above
                if (field.Key.Equals("customData", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!DefaultFields.Contains(field.Key))
                {
                    TryAdd(customFields, field.Key, field.Value?.ToString());
                }

                TryAdd(sanitizedFormData, field.Key, field.Value?.ToString());
            }

            var allNonEmptyFieldNames = sanitizedFormData
                .Where(f => !string.IsNullOrEmpty(f.Value.ToString()))
                .Select(f => f.Key)
                .Distinct()
                .ToArray();

            var jsonErrorHandler = new Func<string, CancellationToken, Task>((message, ct) 
                => Error.Create(context, new BadRequest(message), ct));

            var newAccount = await InstantiateLocalAccount(
                context,
                model,
                allNonEmptyFieldNames,
                customFields,
                client,
                jsonErrorHandler,
                cancellationToken);
            if (newAccount == null)
            {
                return true; // Some error occurred and the handler was invoked
            }

            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);
            var executor = new RegisterExecutor(client, _configuration, _handlers, _logger);

            var formDataForHandler = sanitizedFormData
                .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString());

            var createdAccount = await executor.HandleRegistrationAsync(
                context,
                application,
                formDataForHandler,
                newAccount,
                jsonErrorHandler,
                cancellationToken);
            if (createdAccount == null)
            {
                return true; // Some error occurred and the handler was invoked
            }

            await executor.HandlePostRegistrationAsync(context, createdAccount, cancellationToken);

            var sanitizer = new AccountResponseSanitizer();
            var responseModel = new
            {
                account = sanitizer.Sanitize(createdAccount)
            };

            return await JsonResponse.Ok(context, responseModel);
        }
    }
}
