// <copyright file="Cookie.cs" company="Stormpath, Inc.">
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
using System.Globalization;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class Cookie
    {
        public static string DateFormat = "ddd, dd-MMM-yyyy HH:mm:ss"; // + GMT

        public static string FormatDate(DateTimeOffset dateTimeOffset)
            => $"{dateTimeOffset.UtcDateTime.ToString(DateFormat, CultureInfo.InvariantCulture)} GMT";
    }
}
