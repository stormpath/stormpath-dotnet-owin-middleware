using System.Text;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.SDK.Client;
using Stormpath.SDK.Jwt;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware
{
    public sealed class RedirectTokenParser
    {
        public RedirectTokenParser(
            IClient client,
            ClientApiKeyConfiguration apiKeyConfiguration,
            string token,
            ILogger logger)
        {
            DecodeToken(client, apiKeyConfiguration, token, logger);
        }

        public string Path { get; private set; }

        public string State { get; private set; }

        public bool Valid { get; private set; } = false;

        private  void DecodeToken(
            IClient client,
            ClientApiKeyConfiguration apiKeyConfiguration,
            string token,
            ILogger logger)
        {
            if (string.IsNullOrEmpty(token))
            {
                Valid = false;
                return;
            }

            try
            {
                // TODO: replace with direct JWT library access
                var parsedJwt = client.NewJwtParser()
                    .SetSigningKey(apiKeyConfiguration.Secret, Encoding.UTF8)
                    .Parse(token);

                if (parsedJwt.Body.ContainsClaim(RedirectTokenBuilder.PathClaimName))
                {
                    Path = parsedJwt.Body.GetClaim(RedirectTokenBuilder.PathClaimName).ToString();
                }

                if (parsedJwt.Body.ContainsClaim(RedirectTokenBuilder.StateClaimName))
                {
                    State = parsedJwt.Body.GetClaim(RedirectTokenBuilder.StateClaimName).ToString();
                }

                Valid = true;
            }
            catch (InvalidJwtException ije)
            {
                logger.Warn($"Redirect token failed validation ({ije.Message}): {token}", source: nameof(RedirectTokenParser));
                Valid = false;
            }
        }
    }
}
