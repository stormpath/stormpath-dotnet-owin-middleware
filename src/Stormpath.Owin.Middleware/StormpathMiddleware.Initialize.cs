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
using Microsoft.Extensions.Logging;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Route;
using Stormpath.Configuration;
using Stormpath.Owin.Middleware.Okta;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;

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

            options.Logger = options.Logger ?? NullLogger.Instance;

            options.Logger.LogInformation("Stormpath middleware starting up", nameof(StormpathMiddleware));

            var baseConfiguration = ConfigurationLoader.Initialize().Load(options.Configuration);
            ThrowIfOktaConfigurationMissing(baseConfiguration);
            ThrowIfConfigurationInconsistent(baseConfiguration);

            var oktaClient = new OktaClient(
                baseConfiguration.Org,
                baseConfiguration.ApiToken,
                userAgentBuilder,
                options.CacheProvider,
                options.CacheEntryOptions,
                options.Logger);

            var integrationConfiguration = GetAdditionalConfigFromServer(baseConfiguration, oktaClient, options.Logger);

            var oidcConfigurationEndpoint = $"{integrationConfiguration.Org}/oauth2/{integrationConfiguration.OktaEnvironment.AuthorizationServerId}/.well-known/openid-configuration?client_id={integrationConfiguration.OktaEnvironment.ClientId}";
            var jwksKeyProvider = new CachingJwksKeyProvider(oidcConfigurationEndpoint, options.Logger);

            options.Logger.LogTrace("Stormpath middleware ready!", nameof(StormpathMiddleware));

            var handlerConfiguration = new HandlerConfiguration(
                options.PreChangePasswordHandler ?? DefaultHandlers.PreChangePasswordHandler,
                options.PostChangePasswordHandler ?? DefaultHandlers.PostChangePasswordHandler,
                options.PreLoginHandler ?? DefaultHandlers.PreLoginHandler,
                options.PostLoginHandler ?? DefaultHandlers.PostLoginHandler,
                options.PreLogoutHandler ?? DefaultHandlers.PreLogoutHandler,
                options.PostLogoutHandler ?? DefaultHandlers.PostLogoutHandler,
                options.PreRegistrationHandler ?? DefaultHandlers.PreRegistrationHandler,
                options.PostRegistrationHandler ?? DefaultHandlers.PostRegistrationHandler,
                options.PreVerifyEmailHandler ?? DefaultHandlers.PreVerifyEmailHandler,
                options.PostVerifyEmailHandler ?? DefaultHandlers.PostVerifyEmailHandler,
                options.SendVerificationEmailHandler ?? DefaultHandlers.SendVerificationEmailHandler);

            var authFilterFactory = new DefaultAuthorizationFilterFactory(oktaClient);

            var errorTranslator = options.FriendlyErrorTranslator ?? new OktaFriendlyErrorTranslator();

            return new StormpathMiddleware(
                jwksKeyProvider,
                options.ViewRenderer,
                options.Logger,
                userAgentBuilder,
                integrationConfiguration,
                handlerConfiguration,
                authFilterFactory,
                oktaClient,
                errorTranslator);
        }

        private static void ThrowIfOktaConfigurationMissing(StormpathConfiguration config)
        {
            if (string.IsNullOrEmpty(config?.ApiToken))
            {
                throw new ArgumentNullException("okta.apiToken");
            }

            if (string.IsNullOrEmpty(config?.Org))
            {
                throw new ArgumentNullException("okta.okta.org");
            }

            if (string.IsNullOrEmpty(config?.Application?.Id))
            {
                throw new ArgumentNullException("okta.application.id");
            }
        }

        private static void ThrowIfConfigurationInconsistent(StormpathConfiguration config)
        {
            if (config.Web.Register.AutoLogin && config.Web.Register.EmailVerificationRequired)
            {
                throw new InvalidOperationException("AutoLogin and EmailVerificationRequired cannot both be true for the Register route.");
            }
        }

        private static IntegrationConfiguration GetAdditionalConfigFromServer(
            StormpathConfiguration existingConfig,
            OktaClient client,
            ILogger logger)
        {
            try
            {
                var appDetails = client.GetApplicationAsync(existingConfig.Application.Id, CancellationToken.None).Result;
                var credentials = client.GetClientCredentialsAsync(existingConfig.Application.Id, CancellationToken.None).Result;

                if (string.IsNullOrEmpty(appDetails?.Settings?.Notifications?.Vpn?.Message))
                {
                    throw new ArgumentNullException("The Okta application must be configured with a link to the Authorization Server");
                }

                if (string.IsNullOrEmpty(credentials?.ClientId) || string.IsNullOrEmpty(credentials?.ClientSecret))
                {
                    throw new ArgumentNullException("The Okta application must be configured with a Client ID and Secret");
                }

                logger.LogInformation($"Using Okta application '{appDetails.Label}'");

                var authServerId = appDetails.Settings.Notifications.Vpn.Message; // Workaround to store AS id in app resource
                var authServer = client.GetAuthorizationServerAsync(authServerId, CancellationToken.None).Result;

                logger.LogInformation($"Using authorization server '{authServer.Name}'");

                var idps = client.GetIdentityProvidersAsync(CancellationToken.None).Result;
                var idpProviders = new Dictionary<string, ProviderConfiguration>(StringComparer.OrdinalIgnoreCase);
                logger.LogInformation($"Adding {idps.Length} social providers");

                foreach (var idp in idps)
                {
                    if (string.IsNullOrEmpty(idp?.Links?.Authorize?.Href)) continue;

                    existingConfig.Web.Social.TryGetValue(idp.Type, out var userConfig);

                    idpProviders.Add(idp.Id, new ProviderConfiguration(
                        idp.Type,
                        userConfig?.DisplayName,
                        PatchAuthorizeUri(idp.Links.Authorize.Href, authServerId),
                        userConfig?.Scope));
                }

                var stormpathCallbackAbsoluteUri = string.Empty;
                if (idpProviders.Any())
                {
                    if (string.IsNullOrEmpty(existingConfig.Web.ServerUri))
                    {
                        throw new ArgumentException("The web.serverUri property must be set to your server's absolute base URI when using social login providers.");
                    }

                    stormpathCallbackAbsoluteUri = BuildSafeUrl(existingConfig.Web.ServerUri, existingConfig.Web.Callback.Uri);
                }

                return new IntegrationConfiguration(
                    existingConfig,
                    new OktaEnvironmentConfiguration(
                        authServer.Id,
                        authServer.Audiences,
                        credentials.ClientId,
                        credentials.ClientSecret),
                    idpProviders,
                    stormpathCallbackAbsoluteUri);
            }
            catch (Exception ex)
            {
                logger.LogCritical(1000, ex, "Could not get application information from Okta");
                throw new Exception("Could not get application information from Okta", ex);
            }
        }

        private static string PatchAuthorizeUri(string uri, string authServerId)
            => uri?.Replace("/oauth2/v1/", $"/oauth2/{authServerId}/v1/");

        private AbstractRoute InitializeRoute<T>(RouteOptionsBase options = null)
            where T : AbstractRoute, new()
        {
            var route = new T();
            options = options ?? new RouteOptionsBase();

            route.Initialize(Configuration, Handlers, _viewRenderer, _logger, options, Client, _errorTranslator);

            return route;
        }

        private IReadOnlyDictionary<string, RouteHandler> BuildRoutingTable()
        {
            var routing = new Dictionary<string, RouteHandler>(StringComparer.Ordinal);

            // /oauth/token
            if (Configuration.Web.Oauth2.Enabled)
            {
                _logger.LogInformation($"Oauth2 route enabled on {Configuration.Web.Oauth2.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Oauth2.Uri,
                    new RouteHandler(() => InitializeRoute<Oauth2Route>().InvokeAsync));
            }

            // /stormpathCallback
            if (Configuration.Web.Callback.Enabled)
            {
                _logger.LogInformation($"Callback enabled on {Configuration.Web.Callback.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Callback.Uri,
                    new RouteHandler(() => InitializeRoute<StormpathCallbackRoute>().InvokeAsync));
            }

            // /register
            if (Configuration.Web.Register.Enabled)
            {
                _logger.LogInformation($"Register route enabled on {Configuration.Web.Register.Uri}",
                    nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Register.Uri,
                    new RouteHandler(() => InitializeRoute<RegisterRoute>().InvokeAsync));
            }

            // /login
            if (Configuration.Web.Login.Enabled)
            {
                _logger.LogInformation($"Login route enabled on {Configuration.Web.Login.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Login.Uri,
                    new RouteHandler(() => InitializeRoute<LoginRoute>().InvokeAsync));
            }

            // /me
            if (Configuration.Web.Me.Enabled)
            {
                _logger.LogInformation($"Me route enabled on {Configuration.Web.Me.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Me.Uri,
                    new RouteHandler(() => InitializeRoute<MeRoute>().InvokeAsync, true));
            }

            // /logout
            if (Configuration.Web.Logout.Enabled)
            {
                _logger.LogInformation($"Logout route enabled on {Configuration.Web.Logout.Uri}",
                    nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Logout.Uri,
                    new RouteHandler(() => InitializeRoute<LogoutRoute>().InvokeAsync));
            }

            // /forgot   
            if (ForgotPasswordRoute.ShouldBeEnabled(Configuration))
            {
                _logger.LogInformation($"ForgotPassword route enabled on {Configuration.Web.ForgotPassword.Uri}",
                    nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.ForgotPassword.Uri,
                    new RouteHandler(() => InitializeRoute<ForgotPasswordRoute>().InvokeAsync));
            }

            // /change
            if (ChangePasswordRoute.ShouldBeEnabled(Configuration))
            {
                _logger.LogInformation($"ChangePassword route enabled on {Configuration.Web.ChangePassword.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.ChangePassword.Uri,
                    new RouteHandler(() => InitializeRoute<ChangePasswordRoute>().InvokeAsync));
            }

            // /verify
            if (VerifyEmailRoute.ShouldBeEnabled(Configuration))
            {
                _logger.LogInformation($"VerifyEmail route enabled on {Configuration.Web.VerifyEmail.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.VerifyEmail.Uri,
                    new RouteHandler(() => InitializeRoute<VerifyEmailRoute>().InvokeAsync));
            }

            return routing;
        }

        private static string BuildSafeUrl(string baseUri, string route)
            => $"{baseUri.TrimEnd('/')}/{route.TrimStart('/')}";
    }
}
