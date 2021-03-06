﻿// <copyright file="DefaultFrameworkUserAgentBuilder.cs" company="Stormpath, Inc.">
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
    internal sealed class DefaultFrameworkUserAgentBuilder : IFrameworkUserAgentBuilder
    {
        private readonly Lazy<string> _value;

        public DefaultFrameworkUserAgentBuilder(string runtimeUserAgent)
        {
            _value = new Lazy<string>(() => Generate(runtimeUserAgent));
        }

        public string GetUserAgent() => _value.Value;

        private static string Generate(string runtimeToken)
        {
            var frameworkVersion = typeof(DefaultFrameworkUserAgentBuilder).GetTypeInfo()
                .Assembly
                .GetName()
                .Version;

            var frameworkToken = $"stormpath-owin/{frameworkVersion.Major}.{frameworkVersion.Minor}.{frameworkVersion.Build}";

            return string.Join(" ",
                frameworkToken,
                runtimeToken);
        }
    }
}
