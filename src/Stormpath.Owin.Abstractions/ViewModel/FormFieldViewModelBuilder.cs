// <copyright file="FormFieldViewModelBuilder.cs" company="Stormpath, Inc.">
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
using Stormpath.Configuration.Abstractions.Immutable;

namespace Stormpath.Owin.Common.ViewModel
{
    public class FormFieldViewModelBuilder
    {
        private readonly IReadOnlyList<string> fieldOrder;
        private readonly IReadOnlyDictionary<string, WebFieldConfiguration> fields;

        public FormFieldViewModelBuilder(IReadOnlyList<string> fieldOrder, IReadOnlyDictionary<string, WebFieldConfiguration> fields)
        {
            this.fieldOrder = fieldOrder;
            this.fields = fields;
        }

        public List<FormFieldViewModel> Build()
        {
            var result = new List<FormFieldViewModel>();

            foreach (var fieldName in fieldOrder)
            {
                WebFieldConfiguration field = null;
                if (!fields.TryGetValue(fieldName, out field))
                {
                    throw new Exception($"Invalid field '{fieldName}' in fieldOrder list.");
                }

                if (!field.Enabled)
                {
                    continue;
                }

                result.Add(new FormFieldViewModel()
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
