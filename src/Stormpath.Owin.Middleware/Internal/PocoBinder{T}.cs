// <copyright file="PocoBinder{T}.cs" company="Stormpath, Inc.">
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
using System.Reflection;

namespace Stormpath.Owin.Middleware.Internal
{
    public class PocoBinder<T>
        where T : new()
    {
        private readonly Func<string, string> valueFunc;

        public PocoBinder(Func<string, string> valueFunc)
        {
            this.valueFunc = valueFunc;
        }

        public T Bind()
        {
            var result = new T();

            foreach (var property in typeof(T).GetTypeInfo().DeclaredProperties)
            {
                if (property.PropertyType != typeof(string))
                {
                    throw new Exception($"Unsupported target property type {property.PropertyType.Name}.");
                }

                property.SetValue(result, this.valueFunc(property.Name));
            }

            return result;
        }
    }
}
