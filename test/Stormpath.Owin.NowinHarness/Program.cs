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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Owin;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware;
using Stormpath.Owin.Views.Precompiled;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;

namespace Stormpath.Owin.NowinHarness
{
    using Configuration.Abstractions;
    using SDK.Logging;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class Program
    {
        public static void Main(string[] args)
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
                    Web = new WebConfiguration()
                    {
                        ServerUri = "http://localhost:8080"
                    }
                },
                Logger = logger,
                PreRegistrationHandler = (ctx, ct) =>
                {
                    ctx.Account.CustomData["source"] = "Nowin";
                    return Task.FromResult(true);
                },
                PostRegistrationHandler = async (ctx, ct) =>
                {
                    var customData = await ctx.Account.GetCustomDataAsync(ct);
                },
                PreLoginHandler = (ctx, ct) =>
                {
                    return Task.FromResult(true);
                },
                PostLoginHandler = async (ctx, ct) =>
                {
                    var customData = await ctx.Account.GetCustomDataAsync(ct);
                }
            });

            // Insert it into the OWIN pipeline
            app.Use(stormpath);

            // Add a sample middleware that responds to GET /
            app.Use(new Func<AppFunc, AppFunc>(next => (async env =>
            {
                if (env["owin.RequestPath"] as string != "/")
                {
                    await next.Invoke(env);
                    return;
                }
                using (var writer = new StreamWriter(env["owin.ResponseBody"] as Stream))
                {
                    await writer.WriteAsync("<h1>Hello from OWIN!</h1>");

                    if (!env.ContainsKey(OwinKeys.StormpathUser))
                    {
                        await writer.WriteAsync("<a href=\"/login\">Log in</a> or <a href=\"/register\">Register</a>");
                    }
                    else
                    {
                        var user = env[OwinKeys.StormpathUser] as IAccount;

                        await writer.WriteAsync($"<p>Logged in as {user?.FullName} ({user?.Email})</p>");

                        await writer.WriteAsync(@"
<form action=""/logout"" method=""post"" id=""logout_form"">
  <a onclick=""document.getElementById('logout_form').submit();"" style=""cursor: pointer;"">
    Log Out
  </a>
</form>");
                    }

                    await writer.FlushAsync();
                }
            })));

            // Add a "protected" route
            app.Use(new Func<AppFunc, AppFunc>(next => (async env =>
            {
                if (env["owin.RequestPath"] as string != "/protected")
                {
                    await next.Invoke(env);
                    return;
                }

                if (!env.ContainsKey(OwinKeys.StormpathUser))
                {
                    var deleteCookieAction =
                        new Action<Configuration.Abstractions.Immutable.WebCookieConfiguration>(_ => { }); // TODO
                    var setStatusCodeAction = new Action<int>(code => env["owin.ResponseStatusCode"] = code);
                    var setHeaderAction = new Action<string, string>((name, value) =>
                        (env["owin.ResponseHeaders"] as IDictionary<string, string[]>).SetString(name, value));
                    var redirectAction = new Action<string>(location =>
                    {
                        setStatusCodeAction(302);
                        setHeaderAction("Location", location);
                    });
                    var routeProtector = new RouteProtector(
                        stormpath.GetClient(),
                        stormpath.Configuration,
                        deleteCookieAction,
                        setStatusCodeAction,
                        setHeaderAction,
                        redirectAction,
                        null);
                    routeProtector.OnUnauthorized("text/html", "/protected");
                }
                else
                {
                    using (var writer = new StreamWriter(env["owin.ResponseBody"] as Stream))
                    {
                        await writer.WriteAsync("<p>Zomg secret!</p>");
                        await writer.FlushAsync();
                    }
                }
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
