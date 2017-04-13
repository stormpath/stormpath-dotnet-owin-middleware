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

            options.Logger.LogInformation("Stormpath middleware starting up", nameof(StormpathMiddleware));

            var baseConfiguration = ConfigurationLoader.Initialize().Load(options.Configuration);
            ThrowIfOktaConfigurationMissing(baseConfiguration);
            ThrowIfConfigurationInconsistent(baseConfiguration);
            var oktaClient = new OktaClient(baseConfiguration.Org, baseConfiguration.ApiToken, options.Logger);

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

            return new StormpathMiddleware(
                oktaClient,
                jwksKeyProvider,
                options.ViewRenderer,
                options.Logger,
                userAgentBuilder,
                integrationConfiguration,
                handlerConfiguration);
        }

        private static void ThrowIfOktaConfigurationMissing(StormpathConfiguration config)
        {
            // TODO update after changing configuration model
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

                return new IntegrationConfiguration(
                    existingConfig,
                    new OktaEnvironmentConfiguration(
                        appDetails.Settings.Notifications.Vpn.Message, // Workaround to store AS id in app resource
                        credentials.ClientId,
                        credentials.ClientSecret),
                    new KeyValuePair<string, ProviderConfiguration>[0]);
            }
            catch (Exception ex)
            {
                logger.LogCritical(1000, ex, "Could not get application information from Okta");
                throw new Exception("Could not get application information from Okta", ex);
            }
        }

        private AbstractRoute InitializeRoute<T>(RouteOptionsBase options = null)
            where T : AbstractRoute, new()
        {
            var route = new T();
            options = options ?? new RouteOptionsBase();
            route.Initialize(Configuration, Handlers, viewRenderer, logger, options, oktaClient);

            return route;
        }

        private IReadOnlyDictionary<string, RouteHandler> BuildRoutingTable()
        {
            var routing = new Dictionary<string, RouteHandler>(StringComparer.Ordinal);

            // TODO Absolute URI is required for now, until it can be automatically generated
            var stormpathCallbackAbsoluteUri = new Lazy<string>(() =>
                BuildSafeServerUrl(Configuration.Web, Configuration.Web.Callback.Uri));

            // /oauth/token
            if (Configuration.Web.Oauth2.Enabled)
            {
                logger.LogInformation($"Oauth2 route enabled on {Configuration.Web.Oauth2.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Oauth2.Uri,
                    new RouteHandler(() => InitializeRoute<Oauth2Route>().InvokeAsync));
            }

            // /stormpathCallback
            if (Configuration.Web.Callback.Enabled)
            {
                logger.LogInformation($"Stormpath callback enabled on {Configuration.Web.Callback.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Callback.Uri,
                    new RouteHandler(() => InitializeRoute<StormpathCallbackRoute>().InvokeAsync));
            }

            // /register
            if (Configuration.Web.Register.Enabled)
            {
                logger.LogInformation($"Register route enabled on {Configuration.Web.Register.Uri}",
                    nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Register.Uri,
                    new RouteHandler(() => InitializeRoute<RegisterRoute>().InvokeAsync));
            }

            // /login
            if (Configuration.Web.Login.Enabled)
            {
                logger.LogInformation($"Login route enabled on {Configuration.Web.Login.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Login.Uri,
                    new RouteHandler(() => InitializeRoute<LoginRoute>().InvokeAsync));
            }

            // /me
            if (Configuration.Web.Me.Enabled)
            {
                logger.LogInformation($"Me route enabled on {Configuration.Web.Me.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Me.Uri,
                    new RouteHandler(() => InitializeRoute<MeRoute>().InvokeAsync, true));
            }

            // /logout
            if (Configuration.Web.Logout.Enabled)
            {
                logger.LogInformation($"Logout route enabled on {Configuration.Web.Logout.Uri}",
                    nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.Logout.Uri,
                    new RouteHandler(() => InitializeRoute<LogoutRoute>().InvokeAsync));
            }

            // /forgot   
            if (ForgotPasswordRoute.ShouldBeEnabled(Configuration))
            {
                logger.LogInformation($"ForgotPassword route enabled on {Configuration.Web.ForgotPassword.Uri}",
                    nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.ForgotPassword.Uri,
                    new RouteHandler(() => InitializeRoute<ForgotPasswordRoute>().InvokeAsync));
            }

            // /change
            if (ChangePasswordRoute.ShouldBeEnabled(Configuration))
            {
                logger.LogInformation($"ChangePassword route enabled on {Configuration.Web.ChangePassword.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.ChangePassword.Uri,
                    new RouteHandler(() => InitializeRoute<ChangePasswordRoute>().InvokeAsync));
            }

            // /verify
            if (VerifyEmailRoute.ShouldBeEnabled(Configuration))
            {
                logger.LogInformation($"VerifyEmail route enabled on {Configuration.Web.VerifyEmail.Uri}", nameof(BuildRoutingTable));

                routing.Add(
                    Configuration.Web.VerifyEmail.Uri,
                    new RouteHandler(() => InitializeRoute<VerifyEmailRoute>().InvokeAsync));
            }

            // todo how does social login work?

            //// /callbacks/facebook
            //if (FacebookCallbackRoute.ShouldBeEnabled(Configuration))
            //{
            //    var facebookProvider = Configuration.Providers
            //        .First(p => p.Key.Equals("facebook", StringComparison.OrdinalIgnoreCase))
            //        .Value;

            //    logger.LogInformation($"Facebook callback route enabled on {facebookProvider.CallbackPath}", nameof(BuildRoutingTable));

            //    routing.Add(
            //        facebookProvider.CallbackPath,
            //        new RouteHandler(client => InitializeRoute<FacebookCallbackRoute>(client).InvokeAsync));
            //}

            //// /callbacks/google
            //if (GoogleCallbackRoute.ShouldBeEnabled(Configuration))
            //{
            //    var googleProvider = Configuration.Providers
            //        .First(p => p.Key.Equals("google", StringComparison.OrdinalIgnoreCase))
            //        .Value;

            //    logger.LogInformation($"Google callback route enabled on {googleProvider.CallbackPath}", nameof(BuildRoutingTable));

            //    routing.Add(
            //        googleProvider.CallbackPath,
            //        new RouteHandler(client => InitializeRoute<GoogleCallbackRoute>(client).InvokeAsync));
            //}

            //// /callbacks/github
            //if (GithubCallbackRoute.ShouldBeEnabled(Configuration))
            //{
            //    var githubProvider = Configuration.Providers
            //        .First(p => p.Key.Equals("github", StringComparison.OrdinalIgnoreCase))
            //        .Value;

            //    logger.LogInformation($"Github callback route enabled on {githubProvider.CallbackPath}", nameof(BuildRoutingTable));

            //    routing.Add(
            //        githubProvider.CallbackPath,
            //        new RouteHandler(client => InitializeRoute<GithubCallbackRoute>(client).InvokeAsync));
            //}

            //// /callbacks/linkedin
            //if (LinkedInCallbackRoute.ShouldBeEnabled(Configuration))
            //{
            //    var linkedInProvider = Configuration.Providers
            //        .First(p => p.Key.Equals("linkedin", StringComparison.OrdinalIgnoreCase))
            //        .Value;

            //    logger.LogInformation($"LinkedIn callback route enabled on {linkedInProvider.CallbackPath}", nameof(BuildRoutingTable));

            //    routing.Add(
            //        linkedInProvider.CallbackPath,
            //        new RouteHandler(client => InitializeRoute<LinkedInCallbackRoute>(client).InvokeAsync));
            //}

            return routing;
        }

        private static string BuildSafeServerUrl(WebConfiguration webConfig, string route)
            => $"{webConfig.ServerUri.TrimEnd('/')}/{route.TrimStart('/')}";
    }
}
