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
using Microsoft.Extensions.Logging;


namespace Stormpath.Owin.Middleware.Internal
{
    public sealed class PocoBinder<T>
        where T : new()
    {
        private readonly Func<string, bool> hasValue;
        private readonly Func<string, object> getValue;
        private readonly ILogger logger;

        public PocoBinder(
            Func<string, bool> hasValueFunc,
            Func<string, object> valueFunc,
            ILogger logger)
        {
            if (hasValueFunc == null)
            {
                throw new ArgumentNullException(nameof(hasValueFunc));
            }

            if (valueFunc == null)
            {
                throw new ArgumentNullException(nameof(valueFunc));
            }

            this.hasValue = hasValueFunc;
            this.getValue = valueFunc;
            this.logger = logger;
        }

        public T Bind()
        {
            var result = new T();

            foreach (var property in typeof(T).GetTypeInfo().DeclaredProperties)
            {
                if (property.PropertyType != typeof(string))
                {
                    logger.LogTrace($"Skipping property '{property.Name}' with unsupported type '{property.PropertyType}' while binding {typeof(T).Name}", "PocoBinder.Bind");
                    continue;
                }

                if (!hasValue(property.Name))
                {
                    logger.LogTrace($"Skipping property '{property.Name}' while binding {typeof(T).Name} because no value was found", "PocoBinder.Bind");
                    continue;
                }

                property.SetValue(result, this.getValue(property.Name));
            }

            return result;
        }
    }
}
