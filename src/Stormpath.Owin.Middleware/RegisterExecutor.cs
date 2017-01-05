using System;
using System.Collections.Generic;
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
            IDictionary<string, string> formData,
            IAccount newAccount,
            Func<string, CancellationToken, Task> errorHandler,
            CancellationToken cancellationToken)
        {
            var defaultAccountStore = await application.GetDefaultAccountStoreAsync(cancellationToken);

            var preRegisterHandlerContext = new PreRegistrationContext(environment, newAccount, formData)
            {
                AccountStore = defaultAccountStore as IDirectory
            };

            await _handlers.PreRegistrationHandler(preRegisterHandlerContext, cancellationToken);

            if (preRegisterHandlerContext.Result != null)
            {
                if (!preRegisterHandlerContext.Result.Success)
                {
                    var message = string.IsNullOrEmpty(preRegisterHandlerContext.Result.ErrorMessage)
                        ? "An error has occurred. Please try again."
                        : preRegisterHandlerContext.Result.ErrorMessage;
                    await errorHandler(message, cancellationToken);
                    return null;
                }
            }

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
            Func<string, CancellationToken, Task> errorHandler,
            string stateToken,
            CancellationToken cancellationToken)
        {
            if (_configuration.Web.Register.AutoLogin
                && createdAccount.Status != AccountStatus.Unverified)
            {
                return HandleAutologinAsync(environment, application, errorHandler, postModel, stateToken, cancellationToken);
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
            Func<string, CancellationToken, Task> errorHandler,
            RegisterPostModel postModel,
            string stateToken,
            CancellationToken cancellationToken)
        {
            var loginExecutor = new LoginExecutor(_client, _configuration, _handlers, _logger);
            var loginResult = await loginExecutor.PasswordGrantAsync(
                environment,
                application,
                errorHandler,
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
