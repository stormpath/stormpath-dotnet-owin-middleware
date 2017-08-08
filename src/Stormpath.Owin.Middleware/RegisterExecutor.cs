﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware
{
    internal sealed class RegisterExecutor
    {
        private readonly IntegrationConfiguration _configuration;
        private readonly HandlerConfiguration _handlers;
        private readonly IOktaClient _oktaClient;
        private readonly IFriendlyErrorTranslator _errorTranslator;
        private readonly ILogger _logger;

        public RegisterExecutor(
            IntegrationConfiguration configuration,
            HandlerConfiguration handlers,
            IOktaClient oktaClient,
            IFriendlyErrorTranslator errorTranslator,
            ILogger logger)
        {
            _configuration = configuration;
            _handlers = handlers;
            _oktaClient = oktaClient;
            _errorTranslator = errorTranslator;
            _logger = logger;
        }

        public async Task<ICompatibleOktaAccount> HandleRegistrationAsync(
            IOwinEnvironment environment,
            IDictionary<string, string> formData,
            LocalAccount localProfile,
            string password,
            Func<string, CancellationToken, Task> errorHandler,
            CancellationToken cancellationToken)
        {
            var preRegisterHandlerContext = new PreRegistrationContext(environment, localProfile, formData);

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

            var finalProfile = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(localProfile.FirstName)) finalProfile["firstName"] = localProfile.FirstName;
            if (!string.IsNullOrEmpty(localProfile.MiddleName)) finalProfile["middleName"] = localProfile.MiddleName;
            if (!string.IsNullOrEmpty(localProfile.LastName)) finalProfile["lastName"] = localProfile.LastName;
            if (!string.IsNullOrEmpty(localProfile.Email)) finalProfile["email"] = localProfile.Email;
            if (!string.IsNullOrEmpty(localProfile.Login)) finalProfile["login"] = localProfile.Login;

            // Map CustomData.* to root-level profile fields
            foreach (var item in localProfile.CustomData)
            {
                finalProfile[item.Key] = item.Value;
            }

            // Generate a random code for self-service password reset
            var selfServiceResetCode = CodeGenerator.GetCode();
            finalProfile[Route.ChangePasswordRoute.SelfServiceResetKey] = selfServiceResetCode;

            // Generate a random code for email verification, if necessary
            finalProfile["emailVerificationStatus"] = "UNVERIFIED";
            if (_configuration.Web.Register.EmailVerificationRequired)
            {
                finalProfile["emailVerificationToken"] = CodeGenerator.GetCode();
            }

            User createdUser = null;

            try
            {
                createdUser = await _oktaClient.CreateUserAsync(
                        finalProfile,
                        password,
                        !_configuration.Web.Register.EmailVerificationRequired,
                        "Autogenerated",
                        selfServiceResetCode,
                        cancellationToken);
            }
            catch (OktaException oex)
            {
                var message = _errorTranslator.GetFriendlyMessage(oex);
                await errorHandler(message, cancellationToken);
                // return null below
            }

            if (createdUser == null)
            {
                return null;
            }

            // Assign user to application
            await _oktaClient.AddUserToAppAsync(_configuration.Application.Id, createdUser.Id, localProfile.Email, cancellationToken);

            var stormpathCompatibleUser = new CompatibleOktaAccount(createdUser);

            if (_configuration.Web.Register.EmailVerificationRequired)
            {
                var preVerifyEmailContext = new PreVerifyEmailContext(environment)
                {
                    Email = stormpathCompatibleUser.Email
                };
                await _handlers.PreVerifyEmailHandler(preVerifyEmailContext, cancellationToken);

                var sendVerificationEmailContext = new SendVerificationEmailContext(environment, stormpathCompatibleUser);
                await _handlers.SendVerificationEmailHandler(sendVerificationEmailContext, cancellationToken);
            }

            return stormpathCompatibleUser;
        }

        public async Task HandlePostRegistrationAsync(
            IOwinEnvironment environment, 
            ICompatibleOktaAccount createdAccount,
            CancellationToken cancellationToken)
        {
            var postRegistrationContext = new PostRegistrationContext(environment, createdAccount);
            await _handlers.PostRegistrationHandler(postRegistrationContext, cancellationToken);
        }

        public Task<bool> HandleRedirectAsync(
            IOwinEnvironment environment,
            ICompatibleOktaAccount createdAccount,
            RegisterPostModel postModel,
            Func<string, CancellationToken, Task> errorHandler,
            string stateToken,
            CancellationToken cancellationToken)
        {
            if (_configuration.Web.Register.AutoLogin
                && createdAccount.Status != CompatibleOktaAccount.AccountUnverified)
            {
                return HandleAutologinAsync(environment, errorHandler, postModel, stateToken, cancellationToken);
            }

            string nextUri;
            if (createdAccount.Status == CompatibleOktaAccount.AccountEnabled)
            {
                nextUri = $"{_configuration.Web.Login.Uri}?status=created";
            }
            else if (createdAccount.Status == CompatibleOktaAccount.AccountUnverified)
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
            var loginExecutor = new LoginExecutor(_configuration, _handlers, _oktaClient, _errorTranslator, _logger);
            var (grantResult, user) = await loginExecutor.PasswordGrantAsync(
                environment,
                errorHandler,
                postModel.Email,
                postModel.Password, 
                cancellationToken);

            await loginExecutor.HandlePostLoginAsync(environment, grantResult, user, cancellationToken);

            var parsedStateToken = new StateTokenParser(
                _configuration.Application.Id,
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
