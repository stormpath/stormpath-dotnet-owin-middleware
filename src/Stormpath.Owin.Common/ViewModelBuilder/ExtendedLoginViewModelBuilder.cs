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
using Stormpath.Configuration.Abstractions.Model;
using Stormpath.Owin.Common.ViewModel;

namespace Stormpath.Owin.Common.ViewModelBuilder
{
    public class ExtendedLoginViewModelBuilder
    {
        private readonly WebConfiguration webConfiguration;
        private IDictionary<string, string[]> queryString;
        private IDictionary<string, string[]> previousFormData;

        public ExtendedLoginViewModelBuilder(
            WebConfiguration webConfiguration,
            IDictionary<string, string[]> queryString,
            IDictionary<string, string[]> previousFormData)
        {
            this.webConfiguration = webConfiguration;
            this.queryString = queryString;
            this.previousFormData = previousFormData;
        }

        public ExtendedLoginViewModel Build()
        {
            var baseViewModelBuilder = new LoginViewModelBuilder(this.webConfiguration.Login);
            var result = new ExtendedLoginViewModel(baseViewModelBuilder.Build());

            // Copy values from configuration
            result.ForgotPasswordEnabled = this.webConfiguration.ForgotPassword.Enabled ?? false; // TODO handle null values here
            result.ForgotPasswordUri = this.webConfiguration.ForgotPassword.Uri;
            result.RegistrationEnabled = this.webConfiguration.Register.Enabled ?? false;
            result.RegisterUri = this.webConfiguration.Register.Uri;
            result.VerifyEmailEnabled = this.webConfiguration.VerifyEmail.Enabled ?? false; // TODO handle null values here
            result.VerifyEmailUri = this.webConfiguration.VerifyEmail.Uri;

            // status parameter from queryString
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

            return result;
        }
    }
}
