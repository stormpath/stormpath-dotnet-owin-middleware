using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Model;


namespace Stormpath.Owin.Middleware
{
    internal sealed class RegisterExecutor
    {
        private readonly StormpathConfiguration _configuration;
        private readonly HandlerConfiguration _handlers;
        private readonly ILogger _logger;

        public RegisterExecutor(
            StormpathConfiguration configuration,
            HandlerConfiguration handlers,
            ILogger logger)
        {
            _configuration = configuration;
            _handlers = handlers;
            _logger = logger;
        }

        public async Task<dynamic> HandleRegistrationAsync(
            IOwinEnvironment environment,
            IDictionary<string, string> formData,
            dynamic newAccount,
            Func<string, CancellationToken, Task> errorHandler,
            CancellationToken cancellationToken)
        {
            var preRegisterHandlerContext = new PreRegistrationContext(environment, newAccount, formData);

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

            // todo create an account
            throw new Exception("TODO");
        }

        public async Task HandlePostRegistrationAsync(
            IOwinEnvironment environment, 
            dynamic createdAccount,
            CancellationToken cancellationToken)
        {
            var postRegistrationContext = new PostRegistrationContext(environment, createdAccount);
            await _handlers.PostRegistrationHandler(postRegistrationContext, cancellationToken);
        }

        public Task<bool> HandleRedirectAsync(
            IOwinEnvironment environment,
            dynamic createdAccount,
            RegisterPostModel postModel,
            Func<string, CancellationToken, Task> errorHandler,
            string stateToken,
            CancellationToken cancellationToken)
        {
            // todo some way to check account status
            throw new Exception("TODO");
            //if (_configuration.Web.Register.AutoLogin
            //    && createdAccount.Status != AccountStatus.Unverified)
            //{
            //    return HandleAutologinAsync(environment, errorHandler, postModel, stateToken, cancellationToken);
            //}

            //string nextUri;
            //if (createdAccount.Status == AccountStatus.Enabled)
            //{
            //    nextUri = $"{_configuration.Web.Login.Uri}?status=created";
            //}
            //else if (createdAccount.Status == AccountStatus.Unverified)
            //{
            //    nextUri = $"{_configuration.Web.Login.Uri}?status=unverified";
            //}
            //else
            //{
            //    nextUri = _configuration.Web.Login.Uri;
            //}

            //// Preserve the state token so that the login page can redirect after login if necessary
            //if (!string.IsNullOrEmpty(stateToken))
            //{
            //    if (nextUri.Contains("?"))
            //    {
            //        nextUri += "&";
            //    }
            //    else
            //    {
            //        nextUri += "?";
            //    }

            //    nextUri += $"{StringConstants.StateTokenName}={stateToken}";
            //}

            //return HttpResponse.Redirect(environment, nextUri);
        }

        private async Task<bool> HandleAutologinAsync(
            IOwinEnvironment environment,
            Func<string, CancellationToken, Task> errorHandler,
            RegisterPostModel postModel,
            string stateToken,
            CancellationToken cancellationToken)
        {
            var loginExecutor = new LoginExecutor(_configuration, _handlers, _logger);
            var loginResult = await loginExecutor.PasswordGrantAsync(
                environment,
                errorHandler,
                postModel.Email,
                postModel.Password, 
                cancellationToken);

            await loginExecutor.HandlePostLoginAsync(environment, loginResult, cancellationToken);

            // TODO - use Okta Client secret
            throw new Exception("TODO");

            //var parsedStateToken = new StateTokenParser(oktaClientSecret, stateToken, _logger);
            //return await loginExecutor.HandleRedirectAsync(
            //    environment,
            //    parsedStateToken.Path,
            //    _configuration.Web.Register.NextUri);
        }
    }
}
