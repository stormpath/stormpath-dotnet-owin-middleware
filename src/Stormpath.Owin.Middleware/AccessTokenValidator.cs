﻿using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Okta;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware
{
    public sealed class AccessTokenValidator
    {
        private readonly IOktaClient _oktaClient;
        private readonly IKeyProvider _keyProvider;
        private readonly IntegrationConfiguration _configuration;

        public AccessTokenValidator(
            IOktaClient oktaClient,
            IKeyProvider keyProvider,
            IntegrationConfiguration configuration)
        {
            _oktaClient = oktaClient;
            _keyProvider = keyProvider;
            _configuration = configuration;
        }

        public Task<TokenIntrospectionResult> ValidateAsync(string token, CancellationToken cancellationToken)
        {
            if (_configuration.Web.Oauth2.Password.ValidationStrategy == WebOauth2TokenValidationStrategy.Stormpath)
            {
                var remoteValidator = new RemoteTokenValidator(
                    _oktaClient,
                    _configuration.OktaEnvironment.AuthorizationServerId,
                    _configuration.OktaEnvironment.ClientId,
                    _configuration.OktaEnvironment.ClientSecret);

                return remoteValidator.ValidateAsync(token, TokenType.Access, cancellationToken);
            }

            if (_configuration.Web.Oauth2.Password.ValidationStrategy == WebOauth2TokenValidationStrategy.Local)
            {
                var localValidator = new LocalAccessTokenValidator(
                    _oktaClient,
                    _keyProvider,
                    _configuration.Okta.Org,
                    _configuration.OktaEnvironment.AuthorizationServerId,
                    _configuration.OktaEnvironment.ClientId,
                    _configuration.OktaEnvironment.ClientSecret);

                return localValidator.ValidateAsync(token, cancellationToken);
            }

            throw new InvalidOperationException("The validation strategy is invalid.");
        }
    }
}
