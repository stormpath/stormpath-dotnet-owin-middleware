﻿// <copyright file="Error.cs" company="Stormpath, Inc.">
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
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.SDK.Error;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class Error
    {
        public static Task Create<T>(IOwinEnvironment context, CancellationToken cancellationToken)
            where T : AbstractError, new()
        {
            var instance = new T();

            return Create(context, instance, cancellationToken);
        }

        public static Task Create(IOwinEnvironment context, AbstractError error, CancellationToken cancellationToken)
        {
            context.Response.StatusCode = error.StatusCode;

            if (error.Body != null)
            {
                context.Response.Headers.SetString("Content-Type", Constants.JsonContentType);
                context.Response.Headers.SetString("Cache-Control", "no-store");
                context.Response.Headers.SetString("Pragma", "no-cache");

                return context.Response.WriteAsync(Serializer.Serialize(error.Body), Encoding.UTF8);
            }
            else
            {
                return Task.FromResult(0);
            }
        }

        public static Task CreateFromApiError(IOwinEnvironment context, ResourceException rex, CancellationToken cancellationToken)
        {
            context.Response.StatusCode = rex.HttpStatus;
            context.Response.Headers.SetString("Content-Type", Constants.JsonContentType);
            context.Response.Headers.SetString("Cache-Control", "no-store");
            context.Response.Headers.SetString("Pragma", "no-cache");

            var errorModel = new
            {
                status = rex.HttpStatus,
                message = rex.Message
            };

            return context.Response.WriteAsync(Serializer.Serialize(errorModel), Encoding.UTF8, cancellationToken);
        }
    }
}