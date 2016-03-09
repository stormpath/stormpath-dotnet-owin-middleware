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
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Stormpath.AspNetCore.Model.Error;

namespace Stormpath.AspNetCore
{
    public static class Error
    {
        public static Task Create<T>(HttpContext context)
            where T : AbstractError, new()
        {
            var instance = new T();

            return Create(context, instance);
        }

        public static Task Create(HttpContext context, AbstractError error)
        {
            context.Response.StatusCode = error.StatusCode;

            if (error.Body != null)
            {
                context.Response.ContentType = "application/json;charset=UTF-8";
                context.Response.Headers["Cache-Control"] = "no-store";
                context.Response.Headers["Pragma"] = "no-cache";

                return context.Response.WriteAsync(Serializer.Serialize(error.Body), Encoding.UTF8);
            }
            else
            {
                return Task.FromResult(0);
            }
        }
    }
}