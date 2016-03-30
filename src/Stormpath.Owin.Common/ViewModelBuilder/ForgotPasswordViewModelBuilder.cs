// <copyright file="ForgotPasswordViewModelBuilder.cs" company="Stormpath, Inc.">
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

using System.Collections.Generic;
using Stormpath.Configuration.Abstractions.Model;
using Stormpath.Owin.Common.ViewModel;

namespace Stormpath.Owin.Common.ViewModelBuilder
{
    public class ForgotPasswordViewModelBuilder
    {
        private readonly WebConfiguration webConfiguration;
        private readonly IDictionary<string, string[]> queryString;

        public ForgotPasswordViewModelBuilder(
            WebConfiguration webConfiguration,
            IDictionary<string, string[]> queryString)
        {
            this.webConfiguration = webConfiguration;
            this.queryString = queryString;
        }

        public ForgotPasswordViewModel Build()
        {
            var result = new ForgotPasswordViewModel();

            // status parameter from queryString
            result.Status = this.queryString.GetString("status");

            // Copy values from configuration
            result.ForgotPasswordUri = this.webConfiguration.ForgotPassword.Uri;
            result.LoginEnabled = this.webConfiguration.Login.Enabled;
            result.LoginUri = this.webConfiguration.Login.Uri;

            return result;
        }
    }
}
