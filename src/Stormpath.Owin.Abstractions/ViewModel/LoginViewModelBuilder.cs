﻿// <copyright file="LoginViewModelBuilder.cs" company="Stormpath, Inc.">
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

using Stormpath.Configuration.Abstractions.Immutable;
using System.Linq;

namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class LoginViewModelBuilder
    {
        private readonly WebLoginRouteConfiguration _loginRouteConfiguration;

        public LoginViewModelBuilder(WebLoginRouteConfiguration loginRouteConfiguration)
        {
            _loginRouteConfiguration = loginRouteConfiguration;
        }

        public LoginViewModel Build()
        {
            var result = new LoginViewModel();

            var fieldViewModelBuilder = new FormFieldViewModelBuilder(
                _loginRouteConfiguration.Form.FieldOrder,
                _loginRouteConfiguration.Form.Fields,
                Stormpath.Configuration.Abstractions.Default.Configuration.Web.Login.Form.Fields);
            result.Form.Fields = fieldViewModelBuilder.Build().ToArray();

            return result;
        }
    }
}
