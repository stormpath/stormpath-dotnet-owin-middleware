using Microsoft.Extensions.Logging;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Middleware.Okta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware
{
    public sealed class TokenRevoker
    {
        private readonly IOktaClient _oktaClient;
        private readonly IntegrationConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly List<string> _accessTokensToDelete;
        private readonly List<string> _refreshTokensToDelete;

        public TokenRevoker(
            IOktaClient oktaClient,
            IntegrationConfiguration configuration,
            ILogger logger)
        {
            _oktaClient = oktaClient;
            _configuration = configuration;
            _logger = logger;

            _accessTokensToDelete = new List<string>();
            _refreshTokensToDelete = new List<string>();
        }

        public TokenRevoker AddToken(string token, string tokenType)
        {
            if (string.IsNullOrEmpty(token)) return this;

            if (tokenType == TokenType.Refresh)
            {
                _refreshTokensToDelete.Add(token);
            }
            else
            {
                _accessTokensToDelete.Add(token);
            }

            return this;
        }

        public Task RevokeAsync(CancellationToken cancellationToken)
        {
            var deleteTasks =
                _accessTokensToDelete
                    .Distinct(StringComparer.Ordinal)
                    .Select(token => CallRevokeAsync(token, TokenType.Access, cancellationToken))
            .Concat(
                _refreshTokensToDelete
                    .Distinct(StringComparer.Ordinal)
                    .Select(token => CallRevokeAsync(token, TokenType.Refresh, cancellationToken)));

            return Task.WhenAll(deleteTasks);
        }

        private Task CallRevokeAsync(string token, string tokenType, CancellationToken cancellationToken)
        {
            try
            {
                return _oktaClient.RevokeTokenAsync(
                    _configuration.OktaEnvironment.AuthorizationServerId,
                    _configuration.OktaEnvironment.ClientId,
                    _configuration.OktaEnvironment.ClientSecret,
                    token, tokenType,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(1007, ex, "Error while revoking a token");
                return Task.FromResult(false);
            }
        }
    }
}
