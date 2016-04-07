// <copyright file="Program.cs" company="Stormpath, Inc.">
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
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Owin;

namespace Stormpath.Owin.NowinHarness
{
    using Middleware;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    static class Program
    {
        static void Main(string[] args)
        {
            var options = new StartOptions
            {
                ServerFactory = "Nowin",
                Port = 8080,
            };

            using (WebApp.Start<Startup>(options))
            {
                Console.WriteLine("Running a http server on port 8080");
                Console.ReadKey();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var stormpath = StormpathMiddleware.Create(new StormpathMiddlewareOptions()
            {
                LibraryUserAgent = "nowin/0.22.2",
                ViewRenderer = RenderView,
                Configuration = new
                {
                    application = new
                    {
                        name = "My Application"
                    }
                }
            });
            app.Use(stormpath);

            var testMiddleware = new Func<AppFunc, AppFunc>(TestMiddleware);
            app.Use(testMiddleware);
        }

        public AppFunc TestMiddleware(AppFunc next)
        {
            return async (IDictionary<string, object> environment) =>
            {
                if (environment["owin.RequestPath"] as string == "/foo")
                {
                    // Do something with the incoming request:
                    var response = environment["owin.ResponseBody"] as Stream;
                    using (var writer = new StreamWriter(response))
                    {
                        await writer.WriteAsync("<h1>Hello from My First Middleware</h1>");
                    }
                }

                // Call the next Middleware in the chain:
                await next.Invoke(environment);
            };
        }

        private Task RenderView(string name, object model)
        {
            return Task.FromResult(false);
        }
    }
}
