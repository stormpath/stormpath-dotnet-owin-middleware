// <copyright file="LoginExtendedViewModelBuilder.cs" company="Stormpath, Inc.">
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
using Microsoft.Extensions.Logging;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Abstractions.ViewModel;

namespace Stormpath.Owin.Middleware.ViewModelBuilder
{
    public sealed class LoginFormViewModelBuilder
    {
        private const string OidcResponseType = "code";
        private const string OidcResponseMode = "query";

        private readonly IntegrationConfiguration _configuration;
        private readonly bool _forgotPasswordEnabled;
        private readonly bool _verifyEmailEnabled;
        private readonly IDictionary<string, string[]> _queryString;
        private readonly IDictionary<string, string[]> _previousFormData;
        private readonly IEnumerable<string> _errors;
        private readonly ILogger _logger;

        public LoginFormViewModelBuilder(
            IntegrationConfiguration configuration,
            bool forgotPasswordEnabled,
            bool verifyEmailEnabled,
            IDictionary<string, string[]> queryString,
            IDictionary<string, string[]> previousFormData,
            IEnumerable<string> errors,
            ILogger logger)
        {
            _configuration = configuration;
            _forgotPasswordEnabled = forgotPasswordEnabled;
            _verifyEmailEnabled = verifyEmailEnabled;
            _queryString = queryString ?? new Dictionary<string, string[]>();
            _previousFormData = previousFormData ?? new Dictionary<string, string[]>();
            _errors = errors ?? Enumerable.Empty<string>();
            _logger = logger;
        }

        public LoginFormViewModel Build()
        {
            var baseViewModelBuilder = new LoginViewModelBuilder(_configuration.Web.Login);
            var result = new LoginFormViewModel(baseViewModelBuilder.Build());

            // Copy values from configuration
            result.ForgotPasswordEnabled = _forgotPasswordEnabled;
            result.ForgotPasswordUri = _configuration.Web.ForgotPassword.Uri;
            result.RegistrationEnabled = _configuration.Web.Register.Enabled;
            result.RegisterUri = _configuration.Web.Register.Uri;
            result.VerifyEmailEnabled = _verifyEmailEnabled;
            result.VerifyEmailUri = _configuration.Web.VerifyEmail.Uri;

            // Parameters from querystring
            result.Status = _queryString.GetString("status");
            result.StateToken = _queryString.GetString(StringConstants.StateTokenName);
            // A new state token will be generated if one is not found in the querystring or form, see below

            // Error messages to render
            foreach (var error in _errors)
            {
                result.Errors.Add(error);
            }

            // Previous form submission (if any)
            if (_previousFormData != null && _previousFormData.Any())
            {
                result.FormData = _previousFormData
                    .Where(kvp =>
                    {
                        if (kvp.Key.Equals(StringConstants.StateTokenName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        var definedField = _configuration.Web.Login.Form.Fields.Where(x => x.Key == kvp.Key).SingleOrDefault();
                        bool include = !definedField.Value?.Type.Equals("password", StringComparison.OrdinalIgnoreCase) ?? false;
                        return include;
                    })
                    .ToDictionary(kvp => kvp.Key, kvp => string.Join(",", kvp.Value));
            }

            // If a state token has been previously submitted via form, use that
            string stateTokenFromForm;
            if (result.FormData.TryGetValue(StringConstants.StateTokenName, out stateTokenFromForm)
                && !string.IsNullOrEmpty(stateTokenFromForm))
            {
                result.StateToken = stateTokenFromForm;
            }

            // If a state token exists (from the querystring or a previous submission), make sure it is valid
            if (!string.IsNullOrEmpty(result.StateToken))
            {
                var parsedStateToken = new StateTokenParser(_configuration.Application.Id, _configuration.OktaEnvironment.ClientSecret, result.StateToken, _logger);
                if (!parsedStateToken.Valid)
                {
                    result.StateToken = null; // Will be regenerated below
                }
            }

            // If a state token isn't in the querystring or form, create one
            if (string.IsNullOrEmpty(result.StateToken))
            {
                result.StateToken = new StateTokenBuilder(_configuration.Application.Id, _configuration.OktaEnvironment.ClientSecret).ToString();
            }

            // Social providers
            result.AccountStores = _configuration.Providers.Select(x => new AccountStoreViewModel
                {
                    Type = x.Value.Type,
                    Name = x.Value.DisplayName ?? x.Value.Type,
                    Href = AccountStoreViewModel.CreateUriFromTemplate(
                        template: x.Value.AuthorizeUri,
                        clientId: _configuration.OktaEnvironment.ClientId,
                        responseType: OidcResponseType,
                        responseMode: OidcResponseMode,
                        scopes: x.Value.Scope,
                        redirectUri: _configuration.AbsoluteCallbackUri,
                        state: result.StateToken,
                        nonce: Guid.NewGuid().ToString())
                }).ToArray();

            return result;
        }
    }
}
