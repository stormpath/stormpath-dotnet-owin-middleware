// <copyright file="ChangePasswordViewModelBuilder.cs" company="Stormpath, Inc.">
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

using Stormpath.Configuration.Abstractions.Model;
using Stormpath.Owin.Common.ViewModel;

namespace Stormpath.Owin.Common.ViewModelBuilder
{
    public class ChangePasswordViewModelBuilder
    {
        private readonly WebConfiguration webConfiguration;

        public ChangePasswordViewModelBuilder(
            WebConfiguration webConfiguration)
        {
            this.webConfiguration = webConfiguration;
        }

        public ChangePasswordViewModel Build()
        {
            var result = new ChangePasswordViewModel();

            // Copy values from configuration
            result.ChangePasswordUri = this.webConfiguration.ChangePassword.Uri;

            return result;
        }
    }
}
