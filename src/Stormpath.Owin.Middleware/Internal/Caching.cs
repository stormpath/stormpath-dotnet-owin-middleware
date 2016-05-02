// <copyright file="Caching.cs" company="Stormpath, Inc.">
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

using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class Caching
    {
        public static void AddDoNotCacheHeaders(IOwinEnvironment context)
        {
            context.Response.OnSendingHeaders(state =>
            {
                context.Response.Headers.SetString("Cache-Control", "no-cache, no-store");
                context.Response.Headers.SetString("Pragma", "no-cache");
            }, null);
        }
    }
}
