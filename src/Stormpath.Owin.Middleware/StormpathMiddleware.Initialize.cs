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
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Route;
using Stormpath.SDK;
using Stormpath.SDK.Application;
using Stormpath.SDK.Client;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Http;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Provider;
using Stormpath.SDK.Serialization;
using Stormpath.SDK.Sync;

namespace Stormpath.Owin.Middleware
{
    public sealed partial class StormpathMiddleware
    {
        private static readonly string[] NonSocialProviderIds =
        {
            "stormpath",
            "ad",
            "ldap"
        };

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

            options.Logger.Info("Stormpath middleware starting up", source: nameof(StormpathMiddleware));

            // Initialize and warm up SDK
            options.Logger.Trace("Initializing and warming up SDK...", nameof(StormpathMiddleware));
            var clientFactory = InitializeClient(options.Configuration, options.ConfigurationFileRoot, options.Logger);

            // Scope a client for our resolution steps below
            options.Logger.Trace("Creating scoped ClientFactory...", nameof(StormpathMiddleware));
            var client = clientFactory.Create(new ScopedClientOptions()
            {
                UserAgent = userAgentBuilder.GetUserAgent()
            });

            // Resolve application href, if necessary
            // (see https://github.com/stormpath/stormpath-framework-spec/blob/master/configuration.md#application-resolution)
            options.Logger.Trace("Resolving application...", nameof(StormpathMiddleware));
            var updatedConfiguration = ResolveApplication(client, options.Logger);

            // Pull some configuration from the tenant environment
            options.Logger.Trace("Examining tenant environment...", nameof(StormpathMiddleware));
            var integrationConfiguration = GetIntegrationConfiguration(client, updatedConfiguration, options.Logger);

            // Validate Account Store configuration
            // (see https://github.com/stormpath/stormpath-framework-spec/blob/master/configuration.md#application-resolution)
            options.Logger.Trace("Ensuring the Account Store configuration is valid...", nameof(StormpathMiddleware));
            EnsureAccountStores(client, integrationConfiguration, options.Logger);

            options.Logger.Trace("Stormpath middleware ready!", nameof(StormpathMiddleware));

            return new StormpathMiddleware(options.ViewRenderer, options.Logger, userAgentBuilder, clientFactory,
                integrationConfiguration);
        }

        private static IScopedClientFactory InitializeClient(object initialConfiguration, string configurationFileRoot, ILogger logger)
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
                .SetLogger(logger)
                .Build();

            // Attempt to connect and prime the cache with ITenant
            try
            {
                var tenant = baseClient.GetCurrentTenant();
                if (string.IsNullOrEmpty(tenant?.Href))
                {
                    throw new InitializationException("Could not connect to Stormpath. No tenant could be found.");
                }

                logger.Info($"Using tenant {tenant.Key}", source: nameof(InitializeClient));
            }
            catch (Exception ex)
            {
                throw new InitializationException(
                    "Unable to initialize the Stormpath client. See the inner exception for details.", ex);
            }

            // Scope it!
            return new ScopedClientFactory(baseClient);
        }

        private static StormpathConfiguration ResolveApplication(IClient client, ILogger logger)
        {
            var originalConfiguration = client.Configuration;

            StormpathConfiguration newConfiguration = null;
            bool configurationReady = false;

            // If href is specified, no need to resolve
            if (!string.IsNullOrEmpty(originalConfiguration.Application.Href))
            {
                logger.Trace($"Using provided application href {originalConfiguration.Application.Href}", nameof(ResolveApplication));

                newConfiguration = originalConfiguration;
                configurationReady = true;
            }

            // If name is specified, look up by name
            if (!configurationReady && !string.IsNullOrEmpty(originalConfiguration.Application.Name))
            {
                logger.Trace($"Looking up provided application name '{originalConfiguration.Application.Name}'", nameof(ResolveApplication));

                try
                {
                    var foundApplication = client.GetApplications()
                        .Where(app => app.Name == originalConfiguration.Application.Name)
                        .Synchronously()
                        .Single();

                    logger.Trace($"Application '{foundApplication.Name}' exists at ({foundApplication.Href})", nameof(ResolveApplication));

                    newConfiguration = new StormpathConfiguration(
                        originalConfiguration.Client,
                        new ApplicationConfiguration(href: foundApplication?.Href),
                        originalConfiguration.Web);
                    configurationReady = true;
                }
                catch (Exception ex)
                {
                    throw new InitializationException(
                        $"The provided application could not be found. The provided application name was: {originalConfiguration.Application.Name}",
                        ex);
                }
            }

            // If neither, try to get the single application in the tenant
            if (!configurationReady)
            {
                logger.Trace("No application specified, checking to see if a single application exists", nameof(ResolveApplication));

                try
                {
                    var singleApplication = client.GetApplications()
                        .Synchronously()
                        .Take(3)
                        .ToList()
                        .Single(app => app.Name != "Stormpath");

                    logger.Trace($"Using single application '{singleApplication.Name}' at ({singleApplication.Href})", nameof(ResolveApplication));

                    newConfiguration = new StormpathConfiguration(
                        originalConfiguration.Client,
                        new ApplicationConfiguration(href: singleApplication?.Href),
                        originalConfiguration.Web);
                }
                catch (Exception ex)
                {
                    throw new InitializationException(
                        $"Could not automatically resolve a Stormpath Application. Please specify your Stormpath Application in your configuration.",
                        ex);
                }
            }

            // Attempt to cache the application
            logger.Trace("Ensuring application exists...", nameof(ResolveApplication));
            EnsureApplication(client, newConfiguration, logger);

            return newConfiguration;
        }

        private static void EnsureApplication(IClient client, StormpathConfiguration updatedConfiguration, ILogger logger)
        {
            if (string.IsNullOrEmpty(updatedConfiguration?.Application?.Href))
            {
                throw new InitializationException("The application href is empty.");
            }

            logger.Trace($"Looking up Stormpath application at {updatedConfiguration.Application.Href}");

            try
            {
                var resolvedApplication = client.GetApplication(updatedConfiguration.Application.Href);
                logger.Info($"Using Stormpath application '{resolvedApplication.Name}' ({resolvedApplication.Href})", nameof(EnsureApplication));
            }
            catch (Exception ex)
            {
                throw new InitializationException($"An error occurred when loading the Stormpath application details.", ex);
            }
        }

        private static IntegrationConfiguration GetIntegrationConfiguration(
            IClient client,
            StormpathConfiguration updatedConfiguration,
            ILogger logger)
        {
            var application = client.GetApplication(updatedConfiguration.Application.Href);

            var defaultAccountStore = application.GetDefaultAccountStore();
            var defaultAccountStoreHref = defaultAccountStore?.Href;

            logger.Trace("Default account store href: " + (string.IsNullOrEmpty(defaultAccountStoreHref) ? "<null>" : defaultAccountStoreHref), source: nameof(GetIntegrationConfiguration));

            var defaultAccountStoreDirectory = defaultAccountStore as IDirectory;

            bool emailVerificationEnabled = false;
            bool passwordResetEnabled = false;

            if (defaultAccountStoreDirectory != null)
            {
                var accountCreationPolicy = defaultAccountStoreDirectory.GetAccountCreationPolicy();
                emailVerificationEnabled = accountCreationPolicy.VerificationEmailStatus == SDK.Mail.EmailStatus.Enabled;
                logger.Trace($"Got AccountCreationPolicy. Email workflow enabled: {emailVerificationEnabled}", source: nameof(GetIntegrationConfiguration));

                var passwordPolicy = defaultAccountStoreDirectory.GetPasswordPolicy();
                passwordResetEnabled = passwordPolicy.ResetEmailStatus == SDK.Mail.EmailStatus.Enabled;
                logger.Trace($"Got PasswordPolicy. Reset workflow enabled: {passwordResetEnabled}", source: nameof(GetIntegrationConfiguration));
            }

            logger.Trace("Getting social providers from tenant...", nameof(GetIntegrationConfiguration));

            var socialProviders = GetSocialProviders(application, updatedConfiguration.Web, logger)
                .ToList();

            logger.Trace($"Found {socialProviders.Count} social providers", nameof(GetIntegrationConfiguration));

            return new IntegrationConfiguration(
                updatedConfiguration,
                new TenantConfiguration(defaultAccountStoreHref, emailVerificationEnabled, passwordResetEnabled),
                socialProviders);
        }

        private static IEnumerable<KeyValuePair<string, ProviderConfiguration>> GetSocialProviders(IApplication application, WebConfiguration webConfig, ILogger logger)
        {
            var accountStores = application.GetAccountStoreMappings()
                .Synchronously()
                .ToList()
                .Select(mapping => mapping.GetAccountStore())
                .ToList();

            logger.Trace($"Application has {accountStores.Count} Account Stores", nameof(GetSocialProviders));

            foreach (var accountStore in accountStores)
            {
                if (accountStore == null)
                {
                    logger.Trace("Skipping a null mapped Account Store", nameof(GetSocialProviders));
                    continue;
                }

                var asDirectory = accountStore as IDirectory;
                if (asDirectory == null)
                {
                    logger.Trace($"Account Store is not a directory: {accountStore.Href}", nameof(GetSocialProviders));
                    continue;
                }

                var provider = asDirectory.GetProvider();
                if (NonSocialProviderIds.Any(x => provider.ProviderId.Contains(x)))
                {
                    logger.Trace($"Skipping Account Store {accountStore.Href} with provider ID '{provider.ProviderId}'", nameof(GetSocialProviders));
                    continue;
                }

                logger.Trace($"Getting social provider configuration for Account Store {accountStore.Href}", nameof(GetSocialProviders));
                var providerConfiguration = GetProviderConfiguration(provider, webConfig, logger);

                if (providerConfiguration != null)
                {
                    yield return new KeyValuePair<string, ProviderConfiguration>(
                    provider.ProviderId, providerConfiguration);
                }
            }
        }

        private static ProviderConfiguration GetProviderConfiguration(IProvider provider, WebConfiguration webConfig, ILogger logger)
        {
            var asFacebookProvider = provider as IFacebookProvider;
            if (asFacebookProvider != null)
            {
                WebSocialProviderConfiguration fbConfiguration;
                if (webConfig.Social.TryGetValue("facebook", out fbConfiguration))
                {
                    logger.Trace("Found a Facebook directory, but no stormpath.web.social.facebook configuration exists. Skipping", source: nameof(GetProviderConfiguration));
                    return null;
                }

                logger.Trace($"Facebook directory at {asFacebookProvider.Href}", source: nameof(GetProviderConfiguration));

                return new ProviderConfiguration(
                    asFacebookProvider.ClientId,
                    asFacebookProvider.ClientSecret,
                    callbackPath: fbConfiguration.Uri,
                    callbackUri: fbConfiguration.Uri,
                    scope: fbConfiguration.Scope);
            }

            var asGoogleProvider = provider as IGoogleProvider;
            if (asGoogleProvider != null)
            {
                WebSocialProviderConfiguration googleConfiguration;
                if (!webConfig.Social.TryGetValue("google", out googleConfiguration))
                {
                    logger.Trace("Found a Google directory, but no stormpath.web.social.google configuration exists. Skipping", source: nameof(GetProviderConfiguration));
                    return null;
                }

                logger.Trace($"Google directory at {asGoogleProvider.Href}", source: nameof(GetProviderConfiguration));

                if (string.IsNullOrEmpty(webConfig.ServerUri))
                {
                    throw new InitializationException("The stormpath.web.serverUri property must be set when using Google login integration.");
                }

                var callbackUri = $"{webConfig.ServerUri.TrimEnd('/')}/{googleConfiguration.Uri.TrimStart('/')}";

                return new ProviderConfiguration(
                    asGoogleProvider.ClientId,
                    asGoogleProvider.ClientSecret,
                    callbackPath: googleConfiguration.Uri,
                    callbackUri: callbackUri,
                    scope: googleConfiguration.Scope);
            }

            var asGithubProvider = provider as IGithubProvider;
            if (asGithubProvider != null)
            {
                WebSocialProviderConfiguration githubConfiguration;
                if (!webConfig.Social.TryGetValue("github", out githubConfiguration))
                {
                    logger.Trace("Found a Github directory, but no stormpath.web.social.github configuration exists. Skipping", source: nameof(GetProviderConfiguration));
                    return null;
                }

                logger.Trace($"Github directory at {asGithubProvider.Href}", source: nameof(GetProviderConfiguration));

                if (string.IsNullOrEmpty(webConfig.ServerUri))
                {
                    throw new InitializationException("The stormpath.web.serverUri property must be set when using Github login integration.");
                }

                var callbackUri = $"{webConfig.ServerUri.TrimEnd('/')}/{githubConfiguration.Uri.TrimStart('/')}";

                return new ProviderConfiguration(
                    asGithubProvider.ClientId,
                    asGithubProvider.ClientSecret,
                    callbackPath: githubConfiguration.Uri,
                    callbackUri: callbackUri,
                    scope: githubConfiguration.Scope);
            }

            var asLinkedInProvider = provider as ILinkedInProvider;
            if (asLinkedInProvider != null)
            {
                WebSocialProviderConfiguration linkedinConfiguration;
                if (!webConfig.Social.TryGetValue("linkedin", out linkedinConfiguration))
                {
                    logger.Trace("Found a LinkedIn directory, but no stormpath.web.social.linkedin configuration exists. Skipping", source: nameof(GetProviderConfiguration));
                    return null;
                }

                logger.Trace($"LinkedIn directory at {asLinkedInProvider.Href}", source: nameof(GetProviderConfiguration));

                if (string.IsNullOrEmpty(webConfig.ServerUri))
                {
                    throw new InitializationException("The stormpath.web.serverUri property must be set when using LinkedIn login integration.");
                }

                var callbackUri = $"{webConfig.ServerUri.TrimEnd('/')}/{linkedinConfiguration.Uri.TrimStart('/')}";

                return new ProviderConfiguration(
                    asLinkedInProvider.ClientId,
                    asLinkedInProvider.ClientSecret,
                    callbackPath: linkedinConfiguration.Uri,
                    callbackUri: callbackUri,
                    scope: linkedinConfiguration.Scope);
            }

            logger.Trace($"Provider {provider.Href} has unknown provider ID {provider.ProviderId}. Skipping", source: nameof(GetProviderConfiguration));
            return null;
        }

        private static void EnsureAccountStores(IClient client, IntegrationConfiguration integrationConfiguration, ILogger logger)
        {
            var application = client.GetApplication(integrationConfiguration.Application.Href);

            // The application should have at least one mapped Account Store
            var accountStoreCount = application.GetAccountStoreMappings().Synchronously().Count();
            if (accountStoreCount < 1)
            {
                throw new InitializationException("No account stores are mapped to the specified application. Account stores are required for login and registration.");
            }

            // If the registration route is enabled, we need a default Account Store
            if (integrationConfiguration.Web.Register.Enabled)
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
                this.Configuration,
                this.viewRenderer,
                this.logger,
                client);

            return route;
        }

        private IReadOnlyDictionary<string, RouteHandler> BuildRoutingTable()
        {
            var routing = new Dictionary<string, RouteHandler>(StringComparer.Ordinal);

            // /oauth/token
            if (this.Configuration.Web.Oauth2.Enabled)
            {
                this.logger.Info($"Oauth2 route enabled on {this.Configuration.Web.Oauth2.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.Configuration.Web.Oauth2.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<Oauth2Route>(client).InvokeAsync)
                    );
            }

            // /register
            if (this.Configuration.Web.Register.Enabled)
            {
                this.logger.Info($"Register route enabled on {this.Configuration.Web.Register.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.Configuration.Web.Register.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<RegisterRoute>(client).InvokeAsync)
                    );
            }

            // /login
            if (this.Configuration.Web.Login.Enabled)
            {
                this.logger.Info($"Login route enabled on {this.Configuration.Web.Login.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.Configuration.Web.Login.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<LoginRoute>(client).InvokeAsync)
                    );
            }

            // /me
            if (this.Configuration.Web.Me.Enabled)
            {
                this.logger.Info($"Me route enabled on {this.Configuration.Web.Me.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.Configuration.Web.Me.Uri,
                    new RouteHandler(
                        authenticationRequired: true,
                        handler: client => InitializeRoute<MeRoute>(client).InvokeAsync)
                    );
            }

            // /logout
            if (this.Configuration.Web.Logout.Enabled)
            {
                this.logger.Info($"Logout route enabled on {this.Configuration.Web.Logout.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.Configuration.Web.Logout.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<LogoutRoute>(client).InvokeAsync)
                    );
            }

            // /forgot   
            if (ForgotPasswordRoute.ShouldBeEnabled(this.Configuration))
            {
                this.logger.Info($"ForgotPassword route enabled on {this.Configuration.Web.ForgotPassword.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.Configuration.Web.ForgotPassword.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<ForgotPasswordRoute>(client).InvokeAsync)
                    );
            }

            // /change
            if (ChangePasswordRoute.ShouldBeEnabled(this.Configuration))
            {
                this.logger.Info($"ChangePassword route enabled on {this.Configuration.Web.ChangePassword.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.Configuration.Web.ChangePassword.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<ChangePasswordRoute>(client).InvokeAsync)
                    );
            }

            // /verify
            if (VerifyEmailRoute.ShouldBeEnabled(this.Configuration))
            {
                this.logger.Info($"VerifyEmail route enabled on {this.Configuration.Web.VerifyEmail.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    this.Configuration.Web.VerifyEmail.Uri,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<VerifyEmailRoute>(client).InvokeAsync)
                    );
            }

            // /callbacks/facebook
            if (FacebookCallbackRoute.ShouldBeEnabled(this.Configuration))
            {
                var facebookProvider =this.Configuration.Providers
                    .First(p => p.Key.Equals("facebook", StringComparison.OrdinalIgnoreCase))
                    .Value;

                this.logger.Info($"Facebook callback route enabled on {facebookProvider.CallbackPath}", nameof(BuildRoutingTable));

                routing.Add(
                    facebookProvider.CallbackPath,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<FacebookCallbackRoute>(client).InvokeAsync));
            }

            // /callbacks/google
            if (GoogleCallbackRoute.ShouldBeEnabled(this.Configuration))
            {
                var googleProvider = this.Configuration.Providers
                    .First(p => p.Key.Equals("google", StringComparison.OrdinalIgnoreCase))
                    .Value;

                this.logger.Info($"Google callback route enabled on {googleProvider.CallbackPath}", nameof(BuildRoutingTable));

                routing.Add(
                    googleProvider.CallbackPath,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<GoogleCallbackRoute>(client).InvokeAsync));
            }

            // /callbacks/github
            if (GithubCallbackRoute.ShouldBeEnabled(this.Configuration))
            {
                var githubProvider = this.Configuration.Providers
                    .First(p => p.Key.Equals("github", StringComparison.OrdinalIgnoreCase))
                    .Value;

                this.logger.Info($"Github callback route enabled on {githubProvider.CallbackPath}", nameof(BuildRoutingTable));

                routing.Add(
                    githubProvider.CallbackPath,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<GithubCallbackRoute>(client).InvokeAsync));
            }

            // /callbacks/linkedin
            if (LinkedInCallbackRoute.ShouldBeEnabled(this.Configuration))
            {
                var linkedInProvider = this.Configuration.Providers
                    .First(p => p.Key.Equals("linkedin", StringComparison.OrdinalIgnoreCase))
                    .Value;

                this.logger.Info($"LinkedIn callback route enabled on {linkedInProvider.CallbackPath}", nameof(BuildRoutingTable));

                routing.Add(
                    linkedInProvider.CallbackPath,
                    new RouteHandler(
                        authenticationRequired: false,
                        handler: client => InitializeRoute<LinkedInCallbackRoute>(client).InvokeAsync));
            }

            return routing;
        }
    }
}
