using System;
using System.Collections.Generic;
using JWT;
using JWT.Serializers;
using Microsoft.Extensions.Logging;

namespace Stormpath.Owin.Middleware
{
    public sealed class StateTokenParser
    {
        public StateTokenParser(
            string secret,
            string token,
            ILogger logger)
        {
            if (string.IsNullOrEmpty(secret))
            {
                throw new ArgumentNullException(nameof(secret));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            DecodeToken(secret, token, logger);
        }

        public string Path { get; private set; }

        public string State { get; private set; }

        public bool Valid { get; private set; } = false;

        private void DecodeToken(
            string secret,
            string token,
            ILogger logger)
        {
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            try
            {
                var validator = new JwtValidator(new JsonNetSerializer(), new UtcDateTimeProvider());
                var decoder = new JwtDecoder(new JsonNetSerializer(), validator);

                var payload = decoder.DecodeToObject<IDictionary<string, object>>(token, secret, verify: true);

                if (payload.TryGetValue(StateTokenBuilder.PathClaimName, out object rawPath))
                {
                    Path = rawPath.ToString();
                }

                if (payload.TryGetValue(StateTokenBuilder.StateClaimName, out object rawInnerState))
                {
                    State = rawInnerState.ToString();
                }

                Valid = true;
                return;

            }
            catch (TokenExpiredException)
            {
                logger.LogWarning("State token has expired: {0}", token);
                return;
            }
            catch (SignatureVerificationException)
            {
                logger.LogWarning("State token has invalid signature: {0}", token);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Unexpected error '{0}' while validating state token: {1}", ex.Message, token);
                return;
            }
        }
    }
}
