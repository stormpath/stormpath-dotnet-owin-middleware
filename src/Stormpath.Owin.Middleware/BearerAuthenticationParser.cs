using System;
using Microsoft.Extensions.Logging;


namespace Stormpath.Owin.Middleware
{
    public sealed class BearerAuthenticationParser
    {
        private readonly ILogger _logger;

        public BearerAuthenticationParser(string authorizationHeader, ILogger logger)
        {
            _logger = logger;
            Parse(authorizationHeader);
        }

        public string Token { get; private set; }
        
        public bool IsValid { get; private set; }

        private void Parse(string header)
        {
            var isValid = !string.IsNullOrEmpty(header)
                && header.StartsWith("Bearer ", StringComparison.Ordinal);
            if (!isValid)
            {
                _logger.LogTrace("No Bearer header found", nameof(BearerAuthenticationParser));
                IsValid = false;
                return;
            }

            var bearerPayload = header.Substring(7); // "Bearer " + (payload)
            if (string.IsNullOrEmpty(bearerPayload))
            {
                _logger.LogInformation("Found Bearer header, but payload was empty", nameof(BearerAuthenticationParser));
                IsValid = false;
                return;
            }

            IsValid = true;
            Token = bearerPayload;
        }
    }
}
