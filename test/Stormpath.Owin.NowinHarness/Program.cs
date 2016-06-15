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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Owin;
using Stormpath.Owin.Middleware;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Views.Precompiled;

namespace Stormpath.Owin.NowinHarness
{
    using Configuration.Abstractions;
    using SDK.Logging;
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
            var logger = new ConsoleLogger(LogLevel.Trace);

            // Initialize the Stormpath middleware
            var stormpath = StormpathMiddleware.Create(new StormpathOwinOptions()
            {
                LibraryUserAgent = "nowin/0.22.2",
                ViewRenderer = new PrecompiledViewRenderer(logger),
                Configuration = new StormpathConfiguration
                {
                    Application = new ApplicationConfiguration
                    {
                        Name = "My Application"
                    },
                },
                Logger = logger
            });

            // Insert it into the OWIN pipeline
            app.Use(stormpath);

            // Add a sample middleware that responds to GET /foo
            app.Use(new Func<AppFunc, AppFunc>(next => (async env =>
            {
                if (env["owin.RequestPath"] as string == "/")
                {
                    using (var writer = new StreamWriter(env["owin.ResponseBody"] as Stream))
                    {
                        await writer.WriteAsync("<h1>Hello from OWIN!</h1>");
                        await writer.FlushAsync();
                    }
                }

                await next.Invoke(env);
            })));
        }
    }

    public class ConsoleLogger : ILogger
    {
        private readonly LogLevel level;

        public ConsoleLogger(LogLevel level)
        {
            this.level = level;
        }

        public void Log(LogEntry entry)
        {
            if (entry.Severity < this.level)
            {
                return;
            }

            var message = $"{entry.Severity}: {entry.Source} ";

            if (entry.Exception != null)
            {
                message += $"Exception: {entry.Exception.Message} at {entry.Exception.Source}, ";
            }

            message += $"{entry.Message}";

            Console.WriteLine(message);
        }
    }
}
