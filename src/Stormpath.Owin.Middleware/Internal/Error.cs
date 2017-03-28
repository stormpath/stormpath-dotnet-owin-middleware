// <copyright file="Error.cs" company="Stormpath, Inc.">
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
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Model.Error;
namespace Stormpath.Owin.Middleware.Internal
{
    public static class Error
    {
        public static Task<bool> Create<T>(IOwinEnvironment context, CancellationToken cancellationToken)
            where T : AbstractError, new()
        {
            var instance = new T();

            return Create(context, instance, cancellationToken);
        }

        public static async Task<bool> Create(IOwinEnvironment context, AbstractError error, CancellationToken cancellationToken)
        {
            context.Response.StatusCode = error.StatusCode;

            if (error.Body != null)
            {
                context.Response.Headers.SetString("Content-Type", Constants.JsonContentType);
                Caching.AddDoNotCacheHeaders(context);

                await context.Response.WriteAsync(Serializer.Serialize(error.Body), Encoding.UTF8, cancellationToken);
                return true;
            }
            else
            {
                return true;
            }
        }

        public static async Task<bool> Create(IOwinEnvironment context, int statusCode, string message, CancellationToken cancellationToken)
        {
            context.Response.StatusCode = statusCode;
            context.Response.Headers.SetString("Content-Type", Constants.JsonContentType);
            Caching.AddDoNotCacheHeaders(context);

            var errorModel = new
            {
                status = statusCode,
                message = message
            };

            await context.Response.WriteAsync(Serializer.Serialize(errorModel), Encoding.UTF8, cancellationToken);
            return true;
        }
    }
}
