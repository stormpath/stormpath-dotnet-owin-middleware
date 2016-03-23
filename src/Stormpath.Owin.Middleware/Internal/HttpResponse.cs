// <copyright file="HttpResponse.cs" company="Stormpath, Inc.">
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

using System.Threading.Tasks;
using Stormpath.Owin.Common;
using Stormpath.Owin.Common.View;
using Stormpath.Owin.Middleware.Owin;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class HttpResponse
    {
        public static Task Ok<T>(BaseView<T> view, T viewModel, IOwinEnvironment context)
        {
            context.Response.StatusCode = 200;
            context.Response.Headers.SetString("Content-Type", Constants.HtmlContentType);

            return view.ExecuteAsync(viewModel, context.Response.Body);
        }

        public static Task Redirect(IOwinEnvironment context, string uri)
        {
            context.Response.StatusCode = 302;
            context.Response.Headers.SetString("Location", uri);

            return Task.FromResult(false);
        }
    }
}
