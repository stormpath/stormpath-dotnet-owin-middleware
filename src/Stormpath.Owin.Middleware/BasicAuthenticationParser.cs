using System;
using System.Text;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware
{
    public sealed class BasicAuthenticationParser
    {
        private readonly ILogger _logger;

        public BasicAuthenticationParser(string authorizationHeader, ILogger logger)
        {
            _logger = logger;
            Parse(authorizationHeader);
        }

        public string Username { get; private set; }

        public string Password { get; private set; }

        public bool IsValid { get; private set; }

        private void Parse(string header)
        {
            var isValid = !string.IsNullOrEmpty(header)
                && header.StartsWith("Basic ", StringComparison.Ordinal);
            if (!isValid)
            {
                _logger.Trace("No Basic header found", nameof(BasicAuthenticationParser));
                IsValid = false;
                return;
            }

            var basicPayload = header.Substring(6); // "Basic " + (payload)
            if (string.IsNullOrEmpty(basicPayload))
            {
                _logger.Info("Found Basic header, but payload was empty", nameof(BasicAuthenticationParser));
                IsValid = false;
                return;
            }

            var decodedPayload = string.Empty;

            try
            {
                decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(basicPayload));
            }
            catch (FormatException fex)
            {
                _logger.Info($"Found Basic header, but payload was not valid: {fex.Message}");
                decodedPayload = string.Empty;
            }

            var payloadChunks = decodedPayload.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (payloadChunks.Length != 2)
            {
                _logger.Info("Found Basic header, but it was malformed", nameof(BasicAuthenticationParser));
                IsValid = false;
                return;
            }

            IsValid = true;
            Username = payloadChunks[0];
            Password = payloadChunks[1];
        }
    }
}
