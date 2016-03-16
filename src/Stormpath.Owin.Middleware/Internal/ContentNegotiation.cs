// <copyright file="ContentNegotiation.cs" company="Stormpath, Inc.">
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
using System.Linq;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Middleware.Owin;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class ContentNegotiation
    {
        public static bool IsSupportedByConfiguration(IOwinEnvironment context, StormpathConfiguration configuration)
        {
            // TODO this may need to be fleshed out.

            var acceptHeader = context.Request.Headers.GetString("Accept");

            if (string.IsNullOrEmpty(acceptHeader))
            {
                acceptHeader = "*/*";
            }

            if (acceptHeader.Equals("*/*"))
            {
                return true;
            }

            return configuration.Web.Produces
                .Where(produces => acceptHeader.Contains(produces))
                .Any();
        }

        public static string SelectBestContentType(IOwinEnvironment context, IEnumerable<string> supportedContentTypes)
        {
            // TODO might need to be smarter about parsing the acceptedContentTypes

            var acceptHeader = context.Request.Headers.GetString("Accept");

            bool acceptAny = string.IsNullOrEmpty(acceptHeader) || acceptHeader.Equals("*/*");
            if (acceptAny)
            {
                supportedContentTypes.First();
            }

            var acceptedContentTypes = acceptHeader.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var contentType in acceptedContentTypes)
            {
                if (supportedContentTypes.Contains(contentType.Trim()))
                {
                    return contentType;
                }
            }

            return supportedContentTypes.First();
        }
    }
}
