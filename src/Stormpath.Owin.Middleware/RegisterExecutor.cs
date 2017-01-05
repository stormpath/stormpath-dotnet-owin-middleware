using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.SDK.Account;
using Stormpath.SDK.Application;
using Stormpath.SDK.Client;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Oauth;

namespace Stormpath.Owin.Middleware
{
    internal sealed class RegisterExecutor
    {
        private readonly IClient _client;
        private readonly StormpathConfiguration _configuration;
        private readonly HandlerConfiguration _handlers;
        private readonly ILogger _logger;

        public RegisterExecutor(
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

        public async Task<IAccount> HandleRegistrationAsync(
            IOwinEnvironment environment,
            IApplication application,
            IAccount newAccount,
            CancellationToken cancellationToken)
        {
            var defaultAccountStore = await application.GetDefaultAccountStoreAsync(cancellationToken);

            var preRegisterHandlerContext = new PreRegistrationContext(environment, newAccount)
            {
                AccountStore = defaultAccountStore as IDirectory
            };

            await _handlers.PreRegistrationHandler(preRegisterHandlerContext, cancellationToken);

            IAccount createdAccount;

            if (preRegisterHandlerContext.AccountStore != null)
            {
                createdAccount = await preRegisterHandlerContext.AccountStore.CreateAccountAsync(
                    preRegisterHandlerContext.Account,
                    preRegisterHandlerContext.Options,
                    cancellationToken);
            }
            else
            {
                createdAccount = await application.CreateAccountAsync(
                    preRegisterHandlerContext.Account,
                    preRegisterHandlerContext.Options,
                    cancellationToken);
            }

            return createdAccount;
        }

        public async Task HandlePostRegistrationAsync(
            IOwinEnvironment environment, 
            IAccount createdAccount,
            CancellationToken cancellationToken)
        {
            var postRegistrationContext = new PostRegistrationContext(environment, createdAccount);
            await _handlers.PostRegistrationHandler(postRegistrationContext, cancellationToken);
        }

        public Task<bool> HandleRedirectAsync(
            IOwinEnvironment environment,
            IApplication application,
            IAccount createdAccount,
            RegisterPostModel postModel,
            string stateToken,
            CancellationToken cancellationToken)
        {
            if (_configuration.Web.Register.AutoLogin
                && createdAccount.Status != AccountStatus.Unverified)
            {
                return HandleAutologinAsync(environment, application, createdAccount, postModel, stateToken, cancellationToken);
            }

            string nextUri;
            if (createdAccount.Status == AccountStatus.Enabled)
            {
                nextUri = $"{_configuration.Web.Login.Uri}?status=created";
            }
            else if (createdAccount.Status == AccountStatus.Unverified)
            {
                nextUri = $"{_configuration.Web.Login.Uri}?status=unverified";
            }
            else
            {
                nextUri = _configuration.Web.Login.Uri;
            }

            // Preserve the state token so that the login page can redirect after login if necessary
            if (!string.IsNullOrEmpty(stateToken))
            {
                if (nextUri.Contains("?"))
                {
                    nextUri += "&";
                }
                else
                {
                    nextUri += "?";
                }

                nextUri += $"{StringConstants.StateTokenName}={stateToken}";
            }

            return HttpResponse.Redirect(environment, nextUri);
        }

        private async Task<bool> HandleAutologinAsync(
            IOwinEnvironment environment,
            IApplication application,
            IAccount createdAccount,
            RegisterPostModel postModel,
            string stateToken,
            CancellationToken cancellationToken)
        {
            var loginExecutor = new LoginExecutor(_client, _configuration, _handlers, _logger);
            var loginResult = await loginExecutor.PasswordGrantAsync(
                environment, 
                application, 
                postModel.Email,
                postModel.Password, 
                cancellationToken);

            await loginExecutor.HandlePostLoginAsync(environment, loginResult, cancellationToken);

            var parsedStateToken = new StateTokenParser(_client, _configuration.Client.ApiKey, stateToken, _logger);
            return await loginExecutor.HandleRedirectAsync(
                environment,
                parsedStateToken.Path,
                _configuration.Web.Register.NextUri);
        }
    }
}
