// <copyright file="StormpathMiddleware.Initialize.cs" company="Stormpath, Inc.">
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
using Stormpath.Owin.Common.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK;
using Stormpath.SDK.Client;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Http;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Serialization;
using Stormpath.SDK.Sync;

namespace Stormpath.Owin.Middleware
{
    public sealed partial class StormpathMiddleware
    {
        public static StormpathMiddleware Create(object configuration = null, ILogger logger = null)
        {
            // Construct the base framework User-Agent
            IFrameworkUserAgentBuilder userAgentBuilder = new DefaultFrameworkUserAgentBuilder();

            // Initialize and warm up SDK
            var clientFactory = InitializeClient(configuration);

            // Scope a client for our resolution steps below
            var client = clientFactory.Create(new ScopedClientOptions()
            {
                UserAgent = userAgentBuilder.GetUserAgent()
            });

            // Resolve application href, if necessary
            // (see https://github.com/stormpath/stormpath-framework-spec/blob/master/configuration.md#application-resolution)
            var updatedConfiguration = ResolveApplication(client);

            // Check the tenant environment for 
            var integrationConfiguration = GetIntegrationConfiguration(client, updatedConfiguration);

            // Ensure that the application exists
            EnsureApplication(client, integrationConfiguration);

            // Validate Account Store configuration
            // (see https://github.com/stormpath/stormpath-framework-spec/blob/master/configuration.md#application-resolution)
            EnsureAccountStores(client, integrationConfiguration);

            EnsureEnvironment(client, integrationConfiguration);

            return new StormpathMiddleware(logger, userAgentBuilder, clientFactory, integrationConfiguration);
        }

        private static IScopedClientFactory InitializeClient(object initialConfiguration)
        {
            // Construct base client
            var baseClient = Clients.Builder()
#if NET451
                .SetHttpClient(HttpClients.Create().RestSharpClient())
#else
                .SetHttpClient(HttpClients.Create().SystemNetHttpClient())
#endif
                .SetSerializer(Serializers.Create().JsonNetSerializer())
                .SetConfiguration(initialConfiguration)
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

        private static IntegrationConfiguration GetIntegrationConfiguration(IClient client, StormpathConfiguration updatedConfiguration)
        {
            var application = client.GetApplication(updatedConfiguration.Application.Href);

            var defaultAccountStore = application.GetDefaultAccountStore();
            var defaultAccountStoreHref = defaultAccountStore?.Href;

            var defaultAccountStoreDirectory = defaultAccountStore as IDirectory;

            bool emailVerificationEnabled = false;
            bool passwordResetEnabled = false;

            if (defaultAccountStoreDirectory != null)
            {
                var accountCreationPolicy = defaultAccountStoreDirectory.GetAccountCreationPolicy();
                emailVerificationEnabled = accountCreationPolicy.VerificationEmailStatus == SDK.Mail.EmailStatus.Enabled;

                var passwordPolicy = defaultAccountStoreDirectory.GetPasswordPolicy();
                passwordResetEnabled = passwordPolicy.ResetEmailStatus == SDK.Mail.EmailStatus.Enabled;
            }

            return new IntegrationConfiguration(
                updatedConfiguration,
                new TenantConfiguration(defaultAccountStoreHref, emailVerificationEnabled, passwordResetEnabled));
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

        private static void EnsureAccountStores(IClient client, IntegrationConfiguration integrationConfiguration)
        {
            var application = client.GetApplication(integrationConfiguration.Application.Href);

            // The application should have at least one mapped Account Store
            var accountStoreCount = application.GetAccountStoreMappings().Synchronously().Count();
            if (accountStoreCount < 1)
            {
                throw new InitializationException("No account stores are mapped to the specified application. Account stores are required for login and registration.");
            }

            // If the registration route is enabled, we need a default Account Store
            if (integrationConfiguration.Web.Register.Enabled == true)
            {
                var defaultAccountStore = application.GetDefaultAccountStore();

                if (string.IsNullOrEmpty(integrationConfiguration.Tenant.DefaultAccountStoreHref))
                {
                    throw new InitializationException("No default account store is mapped to the specified application. A default account store is required for registration.");
                }
            }
        }

        private static void EnsureEnvironment(IClient client, IntegrationConfiguration integrationConfiguration)
        {
            // register.autoLogin and email verification workflow should not both be enabled
            if (integrationConfiguration.Web.Register.AutoLogin && integrationConfiguration.Tenant.EmailVerificationWorkflowEnabled)
            {
                throw new InitializationException("Invalid configuration: stormpath.web.register.autoLogin is true, but the default account store of the specified application has the email verification workflow enabled. Auto login is only possible if email verification is disabled. Please disable this workflow on this application's default account store.");
            }


        }
    }
}
