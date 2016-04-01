// <copyright file="StormpathMiddlewareExtensions.cs" company="Stormpath, Inc.">
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
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Stormpath.Owin.Common;
using Stormpath.Owin.Middleware;

namespace Stormpath.Owin.CoreHarness
{
    public static class StormpathMiddlewareExtensions
    {
        /// <summary>
        /// Adds services required for Stormpath.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" />.</param>
        /// <param name="configuration">Configuration options for Stormpath.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <exception cref="InitializationException">There was a problem initializing Stormpath.</exception>
        public static IServiceCollection AddStormpath(this IServiceCollection services, object configuration = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var stormpath = StormpathMiddleware.Create(configuration: configuration);
            services.AddInstance(stormpath);
            services.AddScoped<StormpathClientAccessor>();
            services.AddScoped(provider =>
            {
                var accessor = provider.GetRequiredService<StormpathClientAccessor>();
                return accessor.Get();
            });

            return services;
        }

        /// <summary>
        /// Adds the Stormpath middleware to the pipeline.
        /// </summary>
        /// <remarks>You must call <see cref="AddStormpath(IServiceCollection, object)"/> before calling this method.</remarks>
        /// <param name="app">The <see cref="IApplicationBuilder" />.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <exception cref="InvalidOperationException">The Stormpath services have not been added to the service collection.</exception>
        public static IApplicationBuilder UseStormpath(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var stormpathMiddleware = app.ApplicationServices.GetRequiredService<StormpathMiddleware>();

            //app.UseServices()

            app.UseOwin(addToPipeline =>
            {
                addToPipeline(next =>
                {
                    stormpathMiddleware.Initialize(next);
                    return stormpathMiddleware.Invoke;
                });

                //addToPipeline(next =>
                //{
                //    return OwinHello;
                //});
            });

            return app;
        }

        //public static Task OwinHello(IDictionary<string, object> environment)
        //{
        //    var configuration = environment["Stormpath.Configuration"];

        //    return Task.FromResult(false);
        //}

        public class StormpathUserAccessor
        {
            private readonly IHttpContextAccessor httpContextAccessor;
            private readonly string Id = Guid.NewGuid().ToString();

            public StormpathUserAccessor(IHttpContextAccessor httpContextAccessor)
            {
                this.httpContextAccessor = httpContextAccessor;
            }

            public SDK.Account.IAccount Get()
            {
                var context = this.httpContextAccessor.HttpContext;
                return context.Items[OwinKeys.StormpathUser] as SDK.Account.IAccount;
            }
        }

        public class StormpathClientAccessor
        {
            private readonly IHttpContextAccessor httpContextAccessor;
            private readonly string Id = Guid.NewGuid().ToString();

            public StormpathClientAccessor(IHttpContextAccessor httpContextAccessor)
            {
                this.httpContextAccessor = httpContextAccessor;
            }

            public SDK.Client.IClient Get()
            {
                var context = this.httpContextAccessor.HttpContext;
                return context.Items[OwinKeys.StormpathClient] as SDK.Client.IClient;
            }
        }
    }
}
