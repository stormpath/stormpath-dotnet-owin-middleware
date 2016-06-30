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

namespace Stormpath.Owin.Abstractions.ViewModel
{
    public class ExtendedLoginViewModelBuilder
    {
        private readonly WebConfiguration webConfiguration;
        private readonly IReadOnlyList<KeyValuePair<string, ProviderConfiguration>> providerConfigurations;
        private readonly bool forgotPasswordEnabled;
        private readonly bool verifyEmailEnabled;
        private readonly IDictionary<string, string[]> queryString;
        private readonly IDictionary<string, string[]> previousFormData;

        public ExtendedLoginViewModelBuilder(
            WebConfiguration webConfiguration,
            IReadOnlyList<KeyValuePair<string, ProviderConfiguration>> providerConfigurations,
            bool forgotPasswordEnabled,
            bool verifyEmailEnabled,
            IDictionary<string, string[]> queryString,
            IDictionary<string, string[]> previousFormData)
        {
            this.webConfiguration = webConfiguration;
            this.providerConfigurations = providerConfigurations;
            this.forgotPasswordEnabled = forgotPasswordEnabled;
            this.verifyEmailEnabled = verifyEmailEnabled;
            this.queryString = queryString;
            this.previousFormData = previousFormData;
        }

        public ExtendedLoginViewModel Build()
        {
            var baseViewModelBuilder = new LoginViewModelBuilder(this.webConfiguration.Login);
            var result = new ExtendedLoginViewModel(baseViewModelBuilder.Build());

            // Copy values from configuration
            result.ForgotPasswordEnabled = this.forgotPasswordEnabled;
            result.ForgotPasswordUri = this.webConfiguration.ForgotPassword.Uri;
            result.RegistrationEnabled = this.webConfiguration.Register.Enabled;
            result.RegisterUri = this.webConfiguration.Register.Uri;
            result.VerifyEmailEnabled = this.verifyEmailEnabled;
            result.VerifyEmailUri = this.webConfiguration.VerifyEmail.Uri;

            // Status parameter from queryString
            result.Status = this.queryString.GetString("status");

            // Previous form submission (if any)
            if (this.previousFormData != null)
            {
                result.FormData = previousFormData
                    .Where(kvp =>
                    {
                        var definedField = this.webConfiguration.Login.Form.Fields.Where(x => x.Key == kvp.Key).SingleOrDefault();
                        bool include = !definedField.Value?.Type.Equals("password", StringComparison.OrdinalIgnoreCase) ?? false;
                        return include;
                    })
                    .ToDictionary(kvp => kvp.Key, kvp => string.Join(",", kvp.Value));
            }

            // Social Providers
            result.AccountStores = this.providerConfigurations.Select(x => new AccountStoreViewModel()
            {
                Name = x.Key,
                Href = x.Value.CallbackUri,
                Provider = new AccountStoreProviderViewModel()
                {
                    ClientId = x.Value.ClientId,
                    ProviderId = x.Key,
                    Scope = this.webConfiguration.Social.Get(x.Key)?.Scope
                }
            }).ToArray();

            return result;
        }
    }
}
