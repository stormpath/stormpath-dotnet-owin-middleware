using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    internal sealed class SocialExecutor
    {
        private readonly IntegrationConfiguration _configuration;
        private readonly HandlerConfiguration _handlers;
        private readonly IOktaClient _oktaClient;
        private readonly ILogger _logger;

        public SocialExecutor(
            IntegrationConfiguration configuration,
            HandlerConfiguration handlers,
            IOktaClient oktaClient,
            ILogger logger)
        {
            _configuration = configuration;
            _handlers = handlers;
            _oktaClient = oktaClient;
            _logger = logger;
        }

        public static string CreateErrorUri(WebLoginRouteConfiguration loginRouteConfiguration, string stateToken)
        {
            var uri = $"{loginRouteConfiguration.Uri}?status=social_failed";

            if (!string.IsNullOrEmpty(stateToken))
            {
                uri += $"&{StringConstants.StateTokenName}={stateToken}";
            }

            return uri;
        }

        // todo how will social login work?
        //public async Task<ExternalLoginResult> LoginWithProviderRequestAsync(
        //    IOwinEnvironment environment,
        //    IProviderAccountRequest providerRequest,
        //    CancellationToken cancellationToken)
        //{
        //    var application = await _client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

        //    var result = await application.GetAccountAsync(providerRequest, cancellationToken);

        //    return new ExternalLoginResult
        //    {
        //        Account = result.Account,
        //        IsNewAccount = result.IsNewAccount
        //    };
        //}

        public Task HandleLoginResultAsync(
            IOwinEnvironment environment,
            ExternalLoginResult loginResult,
            CancellationToken cancellationToken)
        {
            // todo how will social login work?
            throw new Exception("TODO");

            //if (loginResult.Account == null)
            //{
            //    throw new ArgumentNullException(nameof(loginResult.Account), "Login resulted in a null account");
            //}

            //var loginExecutor = new LoginExecutor(_configuration, _handlers, _logger);
            //var exchangeResult = await
            //    loginExecutor.TokenExchangeGrantAsync(environment, application, loginResult.Account, cancellationToken);

            //if (exchangeResult == null)
            //{
            //    throw new InvalidOperationException("The token exchange failed");
            //}

            //if (loginResult.IsNewAccount)
            //{
            //    var registerExecutor = new RegisterExecutor(_configuration, _handlers, _logger);
            //    await registerExecutor.HandlePostRegistrationAsync(environment, loginResult.Account, cancellationToken);
            //}

            //await loginExecutor.HandlePostLoginAsync(environment, exchangeResult, cancellationToken);
        }

        public async Task<bool> HandleRedirectAsync(
            IOwinEnvironment environment,
            ExternalLoginResult loginResult,
            string nextUri,
            CancellationToken cancellationToken)
        {
            var loginExecutor = new LoginExecutor(_configuration, _handlers, _oktaClient, _logger);

            var defaultNextPath = loginResult.IsNewAccount
                ? _configuration.Web.Register.NextUri
                : _configuration.Web.Login.NextUri;

            return await loginExecutor.HandleRedirectAsync(environment, nextUri, defaultNextPath);
        }
    }
}
