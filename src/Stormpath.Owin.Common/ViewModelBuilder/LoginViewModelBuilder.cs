// <copyright file="LoginViewModelBuilder.cs" company="Stormpath, Inc.">
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
using Stormpath.Configuration.Abstractions.Model;
using Stormpath.Owin.Common.ViewModel;

namespace Stormpath.Owin.Common.ViewModelBuilder
{
    public sealed class LoginViewModelBuilder
    {
        private readonly WebLoginRouteConfiguration loginRouteConfiguration;

        public LoginViewModelBuilder(WebLoginRouteConfiguration loginRouteConfiguration)
        {
            this.loginRouteConfiguration = loginRouteConfiguration;
        }

        public LoginViewModel Build()
        {
            var result = new LoginViewModel();

            foreach (var fieldName in loginRouteConfiguration.Form.FieldOrder)
            {
                WebFieldConfiguration field = null;
                if (!loginRouteConfiguration.Form.Fields.TryGetValue(fieldName, out field))
                {
                    throw new Exception($"Invalid field '{fieldName}' in fieldOrder list.");
                }

                result.Form.Fields.Add(new FormFieldViewModel()
                {
                    Label = field.Label,
                    Name = fieldName,
                    Placeholder = field.Placeholder,
                    Required = field.Required,
                    Type = field.Type
                });
            }

            return result;
        }
    }
}
