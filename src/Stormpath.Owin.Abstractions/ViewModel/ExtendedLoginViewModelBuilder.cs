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
using System.Runtime.CompilerServices;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.SDK.Client;

namespace Stormpath.Owin.Abstractions.ViewModel
{
    public class ExtendedLoginViewModelBuilder
    {
        private readonly IClient _client;
        private readonly IntegrationConfiguration _configuration;
        private readonly bool _forgotPasswordEnabled;
        private readonly bool _verifyEmailEnabled;
        private readonly IDictionary<string, string[]> _queryString;
        private readonly IDictionary<string, string[]> _previousFormData;
        private readonly IEnumerable<string> _errors;

        public ExtendedLoginViewModelBuilder(
            IClient client, // TODO remove when refactoring JWT
            IntegrationConfiguration configuration,
            bool forgotPasswordEnabled,
            bool verifyEmailEnabled,
            IDictionary<string, string[]> queryString,
            IDictionary<string, string[]> previousFormData,
            IEnumerable<string> errors)
        {
            _client = client;
            _configuration = configuration;
            _forgotPasswordEnabled = forgotPasswordEnabled;
            _verifyEmailEnabled = verifyEmailEnabled;
            _queryString = queryString ?? new Dictionary<string, string[]>();
            _previousFormData = previousFormData ?? new Dictionary<string, string[]>();
            _errors = errors ?? Enumerable.Empty<string>();
        }

        public ExtendedLoginViewModel Build()
        {
            var baseViewModelBuilder = new LoginViewModelBuilder(_configuration.Web.Login, _configuration.Providers);
            var result = new ExtendedLoginViewModel(baseViewModelBuilder.Build());

            // Copy values from configuration
            result.ForgotPasswordEnabled = _forgotPasswordEnabled;
            result.ForgotPasswordUri = _configuration.Web.ForgotPassword.Uri;
            result.RegistrationEnabled = _configuration.Web.Register.Enabled;
            result.RegisterUri = _configuration.Web.Register.Uri;
            result.VerifyEmailEnabled = _verifyEmailEnabled;
            result.VerifyEmailUri = _configuration.Web.VerifyEmail.Uri;

            // Parameters from querystring
            result.Status = _queryString.GetString("status");
            result.StateToken = _queryString.GetString(ExtendedLoginViewModel.DefaultStateTokenName);
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
                        if (kvp.Key.Equals(ExtendedLoginViewModel.DefaultStateTokenName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        var definedField = _configuration.Web.Login.Form.Fields.Where(x => x.Key == kvp.Key).SingleOrDefault();
                        bool include = !definedField.Value?.Type.Equals("password", StringComparison.OrdinalIgnoreCase) ?? false;
                        return include;
                    })
                    .ToDictionary(kvp => kvp.Key, kvp => string.Join(",", kvp.Value));
            }

            // If redirect token has been previously submitted via form, use that
            string stateTokenFromForm;
            if (result.FormData.TryGetValue(ExtendedLoginViewModel.DefaultStateTokenName, out stateTokenFromForm)
                && !string.IsNullOrEmpty(stateTokenFromForm))
            {
                result.StateToken = stateTokenFromForm;
            }

            // If a state token isn't in the querystring or form, create one
            if (string.IsNullOrEmpty(result.StateToken))
            {
                result.StateToken = new StateTokenBuilder(_client, _configuration.Client.ApiKey).ToString();
            }

            return result;
        }
    }
}
