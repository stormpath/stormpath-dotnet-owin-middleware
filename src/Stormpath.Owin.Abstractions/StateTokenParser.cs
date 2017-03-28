using System;
using Microsoft.Extensions.Logging;
namespace Stormpath.Owin.Abstractions
{
    public sealed class StateTokenParser
    {
        public StateTokenParser(
            string secret,
            string token,
            ILogger logger)
        {
            DecodeToken(secret, token, logger);
        }

        public string Path { get; private set; }

        public string State { get; private set; }

        public bool Valid { get; private set; } = false;

        private  void DecodeToken(
            string secret,
            string token,
            ILogger logger)
        {
            if (string.IsNullOrEmpty(token))
            {
                Valid = false;
                return;
            }

            // TODO: replace with direct JWT library access
            throw new Exception("TODO");
            //try
            //{


            //    if (parsedJwt.Body.ContainsClaim(StateTokenBuilder.PathClaimName))
            //    {
            //        Path = parsedJwt.Body.GetClaim(StateTokenBuilder.PathClaimName).ToString();
            //    }

            //    if (parsedJwt.Body.ContainsClaim(StateTokenBuilder.StateClaimName))
            //    {
            //        State = parsedJwt.Body.GetClaim(StateTokenBuilder.StateClaimName).ToString();
            //    }

            //    Valid = true;
            //}
            //catch (InvalidJwtException ije)
            //{
            //    logger.LogWarning($"Redirect token failed validation ({ije.Message}): {token}", source: nameof(StateTokenParser));
            //    Valid = false;
            //}
        }
    }
}
