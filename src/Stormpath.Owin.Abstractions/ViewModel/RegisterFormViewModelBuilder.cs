// <copyright file="ExtendedRegisterViewModelBuilder.cs" company="Stormpath, Inc.">
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
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Abstractions.ViewModel
{
    public class RegisterFormViewModelBuilder
    {
        private readonly IClient _client;
        private readonly IntegrationConfiguration _configuration;
        private readonly IDictionary<string, string[]> _queryString;
        private readonly IDictionary<string, string[]> _previousFormData;
        private readonly ILogger _logger;

        public RegisterFormViewModelBuilder(
            IClient client, // TODO remove when refactoring JWT
            IntegrationConfiguration configuration,
            IDictionary<string, string[]> queryString,
            IDictionary<string, string[]> previousFormData,
            ILogger logger)
        {
            _client = client;
            _configuration = configuration;
            _queryString = queryString;
            _previousFormData = previousFormData;
            _logger = logger;
        }

        public RegisterFormViewModel Build()
        {
            var baseViewModelBuilder = new RegisterViewModelBuilder(_configuration.Web.Register);
            var result = new RegisterFormViewModel(baseViewModelBuilder.Build());

            // Parameters from querystring
            result.StateToken = _queryString.GetString(StringConstants.StateTokenName);
            // A new state token will be generated if one is not found in the querystring or form, see below

            // Copy values from configuration
            result.LoginUri = _configuration.Web.Login.Uri;

            // Previous form submission (if any)
            if (_previousFormData != null)
            {
                result.FormData = _previousFormData
                    .Where(kvp =>
                    {
                        if (kvp.Key.Equals(StringConstants.StateTokenName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        var definedField = _configuration.Web.Register.Form.Fields.Where(x => x.Key == kvp.Key).SingleOrDefault();
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
                var parsedStateToken = new StateTokenParser(_client, _configuration.Client.ApiKey, result.StateToken, _logger);
                if (!parsedStateToken.Valid)
                {
                    result.StateToken = null; // Will be regenerated below
                }
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
