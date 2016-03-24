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

using System.Collections.Generic;
using System.Linq;
using Stormpath.Configuration.Abstractions.Model;
using Stormpath.Owin.Common.ViewModel;

namespace Stormpath.Owin.Common.ViewModelBuilder
{
    public class ExtendedRegisterViewModelBuilder
    {
        private readonly WebConfiguration webConfiguration;
        private IDictionary<string, string[]> previousFormData;

        public ExtendedRegisterViewModelBuilder(
            WebConfiguration webConfiguration,
            IDictionary<string, string[]> previousFormData)
        {
            this.webConfiguration = webConfiguration;
            this.previousFormData = previousFormData;
        }

        public ExtendedRegisterViewModel Build()
        {
            var baseViewModelBuilder = new RegisterViewModelBuilder(this.webConfiguration.Register);
            var result = new ExtendedRegisterViewModel(baseViewModelBuilder.Build());

            // Copy values from configuration
            result.LoginUri = this.webConfiguration.Login.Uri;

            // Previous form submission (if any)
            if (this.previousFormData != null)
            {
                result.FormData = previousFormData.ToDictionary(kvp => kvp.Key, kvp => string.Join(",", kvp.Value));
            }

            return result;
        }
    }
}
