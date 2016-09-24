using System.Threading;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Account;
using Stormpath.SDK.Application;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Provider;

namespace Stormpath.Owin.Middleware
{
    internal sealed class SocialExecutor
    {
        private readonly IClient _client;
        private readonly StormpathConfiguration _configuration;
        private readonly HandlerConfiguration _handlers;
        private readonly ILogger _logger;

        public SocialExecutor(
            IClient client,
            StormpathConfiguration configuration,
            HandlerConfiguration handlers,
            ILogger logger)
        {
            _client = client;
            _configuration = configuration;
            _handlers = handlers;
            _logger = logger;
        }

        public static string GetErrorUri(WebLoginRouteConfiguration loginRouteConfiguration) 
            => $"{loginRouteConfiguration.Uri}?status=social_failed";

        public async Task<ExternalLoginResult> LoginWithProviderRequestAsync(
            IOwinEnvironment environment,
            IProviderAccountRequest providerRequest,
            CancellationToken cancellationToken)
        {
            var application = await _client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            IAccount account;
            var isNewAccount = false;
            try
            {
                var result = await application.GetAccountAsync(providerRequest, cancellationToken);
                account = result.Account;
                isNewAccount = result.IsNewAccount;
            }
            catch (ResourceException rex)
            {
                _logger.Warn(rex, source: nameof(LoginWithProviderRequestAsync));
                account = null;
            }

            return new ExternalLoginResult
            {
                Account = account,
                IsNewAccount = isNewAccount
            };
        }

        public async Task<bool> HandleLoginResultAsync(
            IOwinEnvironment environment,
            IApplication application,
            ExternalLoginResult loginResult,
            CancellationToken cancellationToken)
        {
            if (loginResult.Account == null)
            {
                return await HttpResponse.Redirect(environment, GetErrorUri(_configuration.Web.Login));
            }

            var loginExecutor = new LoginExecutor(_client, _configuration, _handlers, _logger);
            var exchangeResult = await loginExecutor.TokenExchangeGrantAsync(environment, application, loginResult.Account, cancellationToken);

            if (exchangeResult == null)
            {
                return await HttpResponse.Redirect(environment, GetErrorUri(_configuration.Web.Login));
            }

            if (loginResult.IsNewAccount)
            {
                var registerExecutor = new RegisterExecutor(_client, _configuration, _handlers, _logger);
                await registerExecutor.HandlePostRegistrationAsync(environment, loginResult.Account, cancellationToken);
            }

            await loginExecutor.HandlePostLoginAsync(environment, exchangeResult, cancellationToken);

            // TODO: deep link redirection support
            var nextUri = loginResult.IsNewAccount
                ? _configuration.Web.Register.NextUri
                : _configuration.Web.Login.NextUri;

            return await loginExecutor.HandleRedirectAsync(environment, nextUri);
        }
    }
}
