// <copyright file="StormpathMiddleware.cs" company="Stormpath, Inc.">
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
using System.Linq;
using Stormpath.Configuration.Abstractions;
using Stormpath.Configuration.Abstractions.Model;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Route;
using Stormpath.SDK;
using Stormpath.SDK.Client;
using Stormpath.SDK.Http;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Serialization;
using Stormpath.SDK.Sync;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Stormpath.Owin
{
    public sealed class StormpathMiddleware
    {
        private readonly object configuration;
        private readonly ILogger logger;

        public StormpathMiddleware(object configuration = null, ILogger logger = null)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public Action<Action<Func<AppFunc, AppFunc>>> GetPipeline()
        {            
            // Construct the base framework User-Agent
            IFrameworkUserAgentBuilder userAgentBuilder = new DefaultFrameworkUserAgentBuilder();

            // Initialize and warm up SDK
            var clientFactory = InitializeClient();

            // Scope a client for our resolution steps below
            var client = clientFactory.Create(new ScopedClientOptions()
            {
                UserAgent = userAgentBuilder.GetUserAgent()
            });

            // Resolve application href, if necessary
            // (see https://github.com/stormpath/stormpath-framework-spec/blob/master/configuration.md#application-resolution)
            var updatedConfiguration = ResolveApplication(client);

            // Ensure the application exists
            EnsureApplication(client, updatedConfiguration);

            // Validate Account Store configuration
            // (see https://github.com/stormpath/stormpath-framework-spec/blob/master/configuration.md#application-resolution)
            EnsureAccountStores(client, updatedConfiguration);

            // OWINning
            return BuildPipeline(
                updatedConfiguration,
                this.logger,
                clientFactory,
                userAgentBuilder);
        }

        private IScopedClientFactory InitializeClient()
        {
            // Construct base client
            var baseClient = Clients.Builder()
                .SetHttpClient(HttpClients.Create().SystemNetHttpClient())
                .SetSerializer(Serializers.Create().JsonNetSerializer())
                .SetConfiguration(this.configuration)
                .Build();

            // Attempt to connect and prime the cache with ITenant
            try
            {
                var tenant = baseClient.GetCurrentTenant();
            }
            catch (Exception ex)
            {
                throw new InitializationException("Unable to initialize the Stormpath client. See the inner exception for details.", ex);
            }

            // Scope it!
            return new ScopedClientFactory(baseClient);
        }

        private static StormpathConfiguration ResolveApplication(IClient client)
        {
            var originalConfiguration = client.Configuration;

            // If href is specified, no need to resolve
            if (!string.IsNullOrEmpty(originalConfiguration.Application.Href))
            {
                return originalConfiguration;
            }

            // If name is specified, look up by name
            if (!string.IsNullOrEmpty(originalConfiguration.Application.Name))
            {
                try
                {
                    var foundApplication = client.GetApplications()
                        .Where(app => app.Name == originalConfiguration.Application.Name)
                        .Synchronously()
                        .Single();

                    if (string.IsNullOrEmpty(foundApplication.Href))
                    {
                        throw new Exception("The application href is empty."); // Becomes the innerException of the InitializationException
                    }

                    return new StormpathConfiguration(
                        originalConfiguration.Client,
                        new ApplicationConfiguration(href: foundApplication.Href),
                        originalConfiguration.Web);
                }
                catch (Exception ex)
                {
                    throw new InitializationException($"The provided application could not be found. The provided application name was: {originalConfiguration.Application.Name}", ex);
                }
            }

            // If neither, try to get the single application in the tenant
            try
            {
                var singleApplication = client.GetApplications()
                    .Synchronously()
                    .Take(3)
                    .ToList()
                    .Where(app => app.Name != "Stormpath")
                    .Single();

                if (string.IsNullOrEmpty(singleApplication.Href))
                {
                    throw new Exception("The application href is empty."); // Becomes the innerException of the InitializationException
                }

                return new StormpathConfiguration(
                    originalConfiguration.Client,
                    new ApplicationConfiguration(href: singleApplication.Href),
                    originalConfiguration.Web);
            }
            catch (Exception ex)
            {
                throw new InitializationException($"Could not automatically resolve a Stormpath Application. Please specify your Stormpath Application in your configuration.", ex);
            }
        }

        private static void EnsureApplication(IClient client, StormpathConfiguration updatedConfiguration)
        {
            try
            {
                var application = client.GetApplication(updatedConfiguration.Application.Href);

                if (string.IsNullOrEmpty(application.Href))
                {
                    throw new Exception("The application href is empty."); // Becomes the innerException of the InitializationException
                }
            }
            catch (Exception ex)
            {
                throw new InitializationException($"The provided application could not be found. The provided application href was: {updatedConfiguration.Application.Href}", ex);
            }
        }

        private static void EnsureAccountStores(IClient client, StormpathConfiguration updatedConfiguration)
        {
            var application = client.GetApplication(updatedConfiguration.Application.Href);

            // The application should have at least one mapped Account Store
            var accountStoreCount = application.GetAccountStoreMappings().Synchronously().Count();
            if (accountStoreCount < 1)
            {
                throw new InitializationException("No account stores are mapped to the specified application. Account stores are required for login and registration.");
            }

            // register.autoLogin and email verification workflow should not both be enabled
            if (updatedConfiguration.Web.Register.AutoLogin)
            {
                // TODO
            }

            // If the registration route is enabled, we need a default Account Store
            if (updatedConfiguration.Web.Register.Enabled == true)
            {
                var defaultAccountStore = application.GetDefaultAccountStore();

                if (defaultAccountStore == null)
                {
                    throw new InitializationException("No default account store is mapped to the specified application. A default account store is required for registration.");
                }
            }
        }

        private static Action<Action<Func<AppFunc, AppFunc>>> BuildPipeline(StormpathConfiguration config, ILogger logger, IScopedClientFactory clientFactory, IFrameworkUserAgentBuilder userAgentBuilder)
        {
            return new Action<Action<Func<AppFunc, AppFunc>>>(pipeline =>
            {
                if (config.Web.Oauth2.Enabled == true)
                {
                    pipeline(next => new Oauth2Route(next, config, logger, clientFactory, userAgentBuilder, config.Web.Oauth2.Uri).Invoke);
                }

                if (config.Web.Register.Enabled == true)
                {
                    pipeline(next => new RegisterRoute(next, config, logger, clientFactory, userAgentBuilder, config.Web.Register.Uri).Invoke);
                }
            });
        }
    }
}
