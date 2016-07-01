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
using System.Collections.Generic;
using System.Linq;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Route;
using Stormpath.SDK;
using Stormpath.SDK.Application;
using Stormpath.SDK.Client;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Http;
using Stormpath.SDK.Serialization;
using Stormpath.SDK.Sync;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Provider;

namespace Stormpath.Owin.Middleware
{
    public sealed partial class StormpathMiddleware
    {
        public static StormpathMiddleware Create(StormpathOwinOptions options)
        {
            if (string.IsNullOrEmpty(options.LibraryUserAgent))
            {
                throw new ArgumentNullException(nameof(options.LibraryUserAgent));
            }

            if (options.ViewRenderer == null)
            {
                throw new ArgumentNullException(nameof(options.ViewRenderer));
            }

            // Construct the framework User-Agent
            IFrameworkUserAgentBuilder userAgentBuilder = new DefaultFrameworkUserAgentBuilder(options.LibraryUserAgent);

            // Initialize and warm up SDK
            var clientFactory = InitializeClient(options.Configuration, options.ConfigurationFileRoot);

            // Scope a client for our resolution steps below
            var client = clientFactory.Create(new ScopedClientOptions()
            {
                UserAgent = userAgentBuilder.GetUserAgent()
            });

            // Resolve application href, if necessary
            // (see https://github.com/stormpath/stormpath-framework-spec/blob/master/configuration.md#application-resolution)
            var updatedConfiguration = ResolveApplication(client);

            // Pull some configuration from the tenant environment
            var integrationConfiguration = GetIntegrationConfiguration(client, updatedConfiguration);

            // Ensure that the application exists
            EnsureApplication(client, integrationConfiguration);
            options.Logger.Info($"Using Stormpath application {integrationConfiguration.Application.Href}",
                "Initialize.Create");

            // Validate Account Store configuration
            // (see https://github.com/stormpath/stormpath-framework-spec/blob/master/configuration.md#application-resolution)
            EnsureAccountStores(client, integrationConfiguration);

            options.Logger.Trace("Stormpath middleware ready!", "Initialize.Create");
            return new StormpathMiddleware(options.ViewRenderer, options.Logger, userAgentBuilder, clientFactory,
                integrationConfiguration);
        }

        private static IScopedClientFactory InitializeClient(object initialConfiguration, string configurationFileRoot)
        {
            // Construct base client
            var baseClient = Clients.Builder()
#if NET45
                .SetHttpClient(HttpClients.Create().RestSharpClient())
#else
                .SetHttpClient(HttpClients.Create().SystemNetHttpClient())
#endif
                .SetSerializer(Serializers.Create().JsonNetSerializer())
                .SetConfiguration(initialConfiguration)
                .SetConfigurationFileRoot(configurationFileRoot)
                .Build();

            // Attempt to connect and prime the cache with ITenant
            try
            {
                var tenant = baseClient.GetCurrentTenant();
            }
            catch (Exception ex)
            {
                throw new InitializationException(
                    "Unable to initialize the Stormpath client. See the inner exception for details.", ex);
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
                        throw new Exception("The application href is empty.");
                            // Becomes the innerException of the InitializationException
                    }

                    return new StormpathConfiguration(
                        originalConfiguration.Client,
                        new ApplicationConfiguration(href: foundApplication.Href),
                        originalConfiguration.Web);
                }
                catch (Exception ex)
                {
                    throw new InitializationException(
                        $"The provided application could not be found. The provided application name was: {originalConfiguration.Application.Name}",
                        ex);
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
                    throw new Exception("The application href is empty.");
                        // Becomes the innerException of the InitializationException
                }

                return new StormpathConfiguration(
                    originalConfiguration.Client,
                    new ApplicationConfiguration(href: singleApplication.Href),
                    originalConfiguration.Web);
            }
            catch (Exception ex)
            {
                throw new InitializationException(
                    $"Could not automatically resolve a Stormpath Application. Please specify your Stormpath Application in your configuration.",
                    ex);
            }
        }

        private static IntegrationConfiguration GetIntegrationConfiguration(IClient client,
            StormpathConfiguration updatedConfiguration)
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

            var socialProviders = GetSocialProviders(application, updatedConfiguration.Web)
                .ToList();

            return new IntegrationConfiguration(
                updatedConfiguration,
                new TenantConfiguration(defaultAccountStoreHref, emailVerificationEnabled, passwordResetEnabled),
                socialProviders);
        }

        private static readonly string[] NonSocialProviderIds =
        {
            "stormpath",
            "ad",
            "ldap"
        };

        private static IEnumerable<KeyValuePair<string, ProviderConfiguration>> GetSocialProviders(IApplication application, WebConfiguration webConfig)
        {
            var accountStores = application.GetAccountStoreMappings()
                .Synchronously()
                .ToList()
                .Select(mapping => mapping.GetAccountStore())
                .OfType<IDirectory>()
                .ToList();

            foreach (var accountStore in accountStores)
            {
                if (!accountStore.Href.Contains("directories"))
                {
                    continue;
                }

                var provider = accountStore.GetProvider();
                if (NonSocialProviderIds.Any(x => provider.ProviderId.Contains(x)))
                {
                    continue;
                }

                var providerConfiguration = GetProviderConfiguration(provider, webConfig);
                if (providerConfiguration != null)
                {
                    yield return new KeyValuePair<string, ProviderConfiguration>(
                    provider.ProviderId, providerConfiguration);
                }
            }
        }

        private static ProviderConfiguration GetProviderConfiguration(IProvider provider, WebConfiguration webConfig)
        {
            var asFacebookProvider = provider as IFacebookProvider;
            if (asFacebookProvider != null)
            {
                WebSocialProviderConfiguration fbConfiguration;
                if (!webConfig.Social.TryGetValue("facebook", out fbConfiguration))
                {
                    return null;
                }

                return new ProviderConfiguration(
                    asFacebookProvider.ClientId,
                    asFacebookProvider.ClientSecret,
                    fbConfiguration.Uri,
                    fbConfiguration.Scope);
            }

            var asGoogleProvider = provider as IGoogleProvider;
            if (asGoogleProvider != null)
            {
                WebSocialProviderConfiguration googleConfiguration;
                if (!webConfig.Social.TryGetValue("google", out googleConfiguration))
                {
                    return null;
                }

                return new ProviderConfiguration(
                    asGoogleProvider.ClientId,
                    asGoogleProvider.ClientSecret,
                    googleConfiguration.Uri,
                    googleConfiguration.Scope);
            }

            var asGithubProvider = provider as IGithubProvider;
            if (asGithubProvider != null)
            {
                WebSocialProviderConfiguration githubConfiguration;
                if (!webConfig.Social.TryGetValue("github", out githubConfiguration))
                {
                    return null;
                }

                return new ProviderConfiguration(
                    asGithubProvider.ClientId,
                    asGithubProvider.ClientSecret,
                    githubConfiguration.Uri,
                    githubConfiguration.Scope);
            }

            var asLinkedInProvider = provider as ILinkedInProvider;
            if (asLinkedInProvider != null)
            {
                WebSocialProviderConfiguration linkedinConfiguration;
                if (!webConfig.Social.TryGetValue("linkedin", out linkedinConfiguration))
                {
                    return null;
                }

                return new ProviderConfiguration(
                    asLinkedInProvider.ClientId,
                    asLinkedInProvider.ClientSecret,
                    linkedinConfiguration.Uri,
                    linkedinConfiguration.Scope);
            }

            return null;
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

        private AbstractRoute InitializeRoute<T>(IClient client)
            where T : AbstractRoute, new()
        {
            var route = new T();

            route.Initialize(
                this.configuration,
                this.viewRenderer,
                this.logger,
                client);

            return route;
        }

        private IReadOnlyDictionary<string, RouteHandler> BuildRoutingTable()
        {
            var routing = new Dictionary<string, RouteHandler>(StringComparer.Ordinal);

            // /oauth/token
            if (this.configuration.Web.Oauth2.Enabled)
            {
                this.logger.Info($"Oauth2 route enabled on {this.configuration.Web.Oauth2.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.configuration.Web.Oauth2.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<Oauth2Route>(client).InvokeAsync)
                    );
            }

            // /register
            if (this.configuration.Web.Register.Enabled)
            {
                this.logger.Info($"Register route enabled on {this.configuration.Web.Register.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.configuration.Web.Register.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<RegisterRoute>(client).InvokeAsync)
                    );
            }

            // /login
            if (this.configuration.Web.Login.Enabled)
            {
                this.logger.Info($"Login route enabled on {this.configuration.Web.Login.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.configuration.Web.Login.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<LoginRoute>(client).InvokeAsync)
                    );
            }

            // /me
            if (this.configuration.Web.Me.Enabled)
            {
                this.logger.Info($"Me route enabled on {this.configuration.Web.Me.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.configuration.Web.Me.Uri,
                    new RouteHandler(
                        authenticationRequired: true,
                        handler: client => InitializeRoute<MeRoute>(client).InvokeAsync)
                    );
            }

            // /logout
            if (this.configuration.Web.Logout.Enabled)
            {
                this.logger.Info($"Logout route enabled on {this.configuration.Web.Logout.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.configuration.Web.Logout.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<LogoutRoute>(client).InvokeAsync)
                    );
            }

            // /forgot   
            if (ForgotPasswordRoute.ShouldBeEnabled(this.configuration))
            {
                this.logger.Info($"ForgotPassword route enabled on {this.configuration.Web.ForgotPassword.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.configuration.Web.ForgotPassword.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<ForgotPasswordRoute>(client).InvokeAsync)
                    );
            }

            // /change
            if (ChangePasswordRoute.ShouldBeEnabled(this.configuration))
            {
                this.logger.Info($"ChangePassword route enabled on {this.configuration.Web.ChangePassword.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.configuration.Web.ChangePassword.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<ChangePasswordRoute>(client).InvokeAsync)
                    );
            }

            // /verify
            if (VerifyEmailRoute.ShouldBeEnabled(this.configuration))
            {
                this.logger.Info($"VerifyEmail route enabled on {this.configuration.Web.VerifyEmail.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.configuration.Web.VerifyEmail.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<VerifyEmailRoute>(client).InvokeAsync)
                    );
            }

            // /callbacks/facebook
            if (FacebookCallbackRoute.ShouldBeEnabled(this.configuration))
            {
                var facebookProvider =this.configuration.Providers
                    .First(p => p.Key.Equals("facebook", StringComparison.OrdinalIgnoreCase))
                    .Value;

                this.logger.Info($"Facebook callback route enabled on {facebookProvider.CallbackUri}", nameof(BuildRoutingTable));

                routing.Add(
                    facebookProvider.CallbackUri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<FacebookCallbackRoute>(client).InvokeAsync));
            }

            // /callbacks/google
            if (GoogleCallbackRoute.ShouldBeEnabled(this.configuration))
            {
                var googleProvider = this.configuration.Providers
                    .First(p => p.Key.Equals("google", StringComparison.OrdinalIgnoreCase))
                    .Value;

                this.logger.Info($"Google callback route enabled on {googleProvider.CallbackUri}", nameof(BuildRoutingTable));

                routing.Add(
                    googleProvider.CallbackUri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<GoogleCallbackRoute>(client).InvokeAsync));
            }

            // /callbacks/github
            if (GithubCallbackRoute.ShouldBeEnabled(this.configuration))
            {
                var githubProvider = this.configuration.Providers
                    .First(p => p.Key.Equals("github", StringComparison.OrdinalIgnoreCase))
                    .Value;

                this.logger.Info($"Github callback route enabled on {githubProvider.CallbackUri}", nameof(BuildRoutingTable));

                routing.Add(
                    githubProvider.CallbackUri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<GithubCallbackRoute>(client).InvokeAsync));
            }

            // /callbacks/linkedin
            if (LinkedInCallbackRoute.ShouldBeEnabled(this.configuration))
            {
                var linkedInProvider = this.configuration.Providers
                    .First(p => p.Key.Equals("linkedin", StringComparison.OrdinalIgnoreCase))
                    .Value;

                this.logger.Info($"LinkedIn callback route enabled on {linkedInProvider.CallbackUri}", nameof(BuildRoutingTable));

                routing.Add(
                    linkedInProvider.CallbackUri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<LinkedInCallbackRoute>(client).InvokeAsync));
            }

            return routing;
        }
    }
}
