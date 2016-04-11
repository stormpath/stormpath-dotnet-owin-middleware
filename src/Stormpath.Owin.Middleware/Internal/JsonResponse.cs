// <copyright file="JsonResponse.cs" company="Stormpath, Inc.">
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

using System.Text;
using System.Threading.Tasks;
using Stormpath.Owin.Common;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class JsonResponse
    {
        public static Task<bool> Ok(IOwinEnvironment context, object model = null)
        {
            context.Response.StatusCode = 200;
            context.Response.Headers.SetString("Content-Type", Constants.JsonContentType);

            return RespondWithOptionalBody(context, model);
        }

        public static Task<bool> Unauthorized(IOwinEnvironment context, object model = null)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.SetString("Content-Type", Constants.JsonContentType);

            return RespondWithOptionalBody(context, model);
        }

        private static async Task<bool> RespondWithOptionalBody(IOwinEnvironment context, object model = null)
        {
            if (model != null)
            {
                await context.Response.WriteAsync(Serializer.Serialize(model), Encoding.UTF8, context.CancellationToken);
            }

            return true;
        }
    }
}
