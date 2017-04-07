﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Okta;
using Stormpath.Owin.Middleware.Internal;

namespace Stormpath.Owin.Middleware
{
    internal sealed class RegisterExecutor
    {
        private readonly IntegrationConfiguration _configuration;
        private readonly HandlerConfiguration _handlers;
        private readonly IOktaClient _oktaClient;
        private readonly ILogger _logger;

        public RegisterExecutor(
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

        public async Task<dynamic> HandleRegistrationAsync(
            IOwinEnvironment environment,
            IDictionary<string, string> formData,
            dynamic newProfile,
            string password,
            Func<string, CancellationToken, Task> errorHandler,
            CancellationToken cancellationToken)
        {
            var preRegisterHandlerContext = new PreRegistrationContext(environment, newProfile, formData);

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

            // Map CustomData.* to root-level profile fields
            var newProfileAsDictionary = (IDictionary<string, object>)newProfile;
            var customDataAsDictionary = (IDictionary<string, object>)newProfile.CustomData;

            foreach (var item in customDataAsDictionary)
            {
                newProfile[item.Key] = item.Value;
            }

            newProfileAsDictionary.Remove("CustomData");

            var createdUser = await _oktaClient.CreateUserAsync(newProfile, password, cancellationToken);
            if (createdUser == null)
            {
                return null;
            }

            // Assign user to application
            await _oktaClient.AddUserToAppAsync(_configuration.Okta.Application.Id, createdUser.Id, newProfile.email, cancellationToken);

            var stormpathCompatibleUser = new StormpathUserTransformer(_logger).OktaToStormpathUser(createdUser);
            return stormpathCompatibleUser;
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
            if (_configuration.Web.Register.AutoLogin
                && createdAccount.Status != StormpathUserTransformer.AccountUnverified)
            {
                return HandleAutologinAsync(environment, errorHandler, postModel, stateToken, cancellationToken);
            }

            string nextUri;
            if (createdAccount.Status == StormpathUserTransformer.AccountEnabled)
            {
                nextUri = $"{_configuration.Web.Login.Uri}?status=created";
            }
            else if (createdAccount.Status == StormpathUserTransformer.AccountUnverified)
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
            Func<string, CancellationToken, Task> errorHandler,
            RegisterPostModel postModel,
            string stateToken,
            CancellationToken cancellationToken)
        {
            var loginExecutor = new LoginExecutor(_configuration, _handlers, _oktaClient, _logger);
            var loginResult = await loginExecutor.PasswordGrantAsync(
                environment,
                errorHandler,
                postModel.Email,
                postModel.Password, 
                cancellationToken);

            await loginExecutor.HandlePostLoginAsync(environment, loginResult, cancellationToken);

            var parsedStateToken = new StateTokenParser(
                _configuration.Okta.Application.Id,
                _configuration.OktaEnvironment.ClientSecret,
                stateToken,
                _logger);

            return await loginExecutor.HandleRedirectAsync(
                environment,
                parsedStateToken.Path,
                _configuration.Web.Register.NextUri);
        }
    }
}
