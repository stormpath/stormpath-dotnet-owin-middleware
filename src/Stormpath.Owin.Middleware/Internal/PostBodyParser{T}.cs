// <copyright file="PostBodyParser{T}.cs" company="Stormpath, Inc.">
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
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class PostBodyParser
    {
        public static async Task<T> ToModel<T>(IOwinEnvironment context, ContentType bodyContentType, ILogger logger, CancellationToken cancellationToken)
            where T : new()
        {
            var bodyString = await context.Request.GetBodyAsStringAsync(cancellationToken);

            return ToModel<T>(bodyString, bodyContentType, logger);
        }

        public static T ToModel<T>(string body, ContentType bodyContentType, ILogger logger)
            where T : new()
        {
            if (bodyContentType == ContentType.Json)
            {
                return Serializer.Deserialize<T>(body);
            }

            if (bodyContentType == ContentType.FormUrlEncoded)
            {
                var formDictionary = FormContentParser.Parse(body, logger);
                return new PocoBinder<T>(key => formDictionary.GetString(key)).Bind();
            }

            // This should never happen. The logic in AbstractRoute should throw first if the Content-Type is unsupported.
            throw new Exception($"Unsupported Content-Type {bodyContentType.ToString()}");
        }
    }
}
