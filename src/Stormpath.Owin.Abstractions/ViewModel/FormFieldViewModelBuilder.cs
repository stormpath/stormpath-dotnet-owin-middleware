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
using System.Linq;
using System.Collections.Generic;
using Stormpath.Configuration.Abstractions.Immutable;

namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class FormFieldViewModelBuilder
    {
        private readonly IReadOnlyList<string> fieldOrder;
        private readonly IReadOnlyDictionary<string, WebFieldConfiguration> fields;
        private readonly IReadOnlyDictionary<string, WebFieldConfiguration> defaultFields; 

        public FormFieldViewModelBuilder(
            IReadOnlyList<string> fieldOrder,
            IReadOnlyDictionary<string, WebFieldConfiguration> fields,
            IReadOnlyDictionary<string, WebFieldConfiguration> defaultFields)
        {
            this.fieldOrder = fieldOrder;
            this.fields = fields;
            this.defaultFields = defaultFields;
        }

        public IEnumerable<FormFieldViewModel> Build()
        {
            // fieldOrder is not guaranteed to list every field, if it's been edited
            var unorderedFields = this.fields.Select(kvp => kvp.Key).Where(f => !fieldOrder.Contains(f));
            var unorderedDefaultFields = this.defaultFields.Select(kvp => kvp.Key).Where(f => !fieldOrder.Contains(f));
            var finalFieldOrdering = new List<string>(fieldOrder
                .Concat(unorderedFields)
                .Concat(unorderedDefaultFields));

            var definitions = finalFieldOrdering.Select(name =>
            {
                WebFieldConfiguration field = null;

                bool isDefinedOrDefault = fields.TryGetValue(name, out field) || defaultFields.TryGetValue(name, out field);
                if (!isDefinedOrDefault)
                {
                    throw new Exception($"Invalid field '{name}' in fieldOrder list.");
                }

                return new { name, field };
            });

            return definitions
                .Where(def => def.field.Enabled && def.field.Visible)
                .Select(def => new FormFieldViewModel()
                {
                    Label = def.field.Label,
                    Name = def.name,
                    Placeholder = def.field.Placeholder,
                    Required = def.field.Required,
                    Type = def.field.Type
                });
        }
    }
}
