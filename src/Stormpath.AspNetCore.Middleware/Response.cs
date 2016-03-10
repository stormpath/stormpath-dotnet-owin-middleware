﻿// <copyright file="Response.cs" company="Stormpath, Inc.">
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

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace Stormpath.AspNetCore
{
    public static class Response
    {
        public static Task Ok(object model, HttpContext context, CancellationToken cancellationToken)
        {
            context.Response.ContentType = "application/json;charset=UTF-8";

            return context.Response.WriteAsync(Serializer.Serialize(model), cancellationToken);
        }
    }
}