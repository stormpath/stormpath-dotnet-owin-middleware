using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace Stormpath.Owin.Middleware
{
    public sealed class TokenRevoker
    {
        private readonly ILogger _logger;
        private readonly List<string> _accessTokenIdsToDelete;
        private readonly List<string> _refreshTokenIdsToDelete;

        public TokenRevoker(ILogger logger)
        {
            _logger = logger;
            _accessTokenIdsToDelete = new List<string>();
            _refreshTokenIdsToDelete = new List<string>();
        }

        // todo rewrite revocation story

        //private bool IsValidJwt(string jwt, out IJwt parsedJwt)
        //{
        //    parsedJwt = null;

        //    if (string.IsNullOrEmpty(jwt))
        //    {
        //        return false;
        //    }

        //    try
        //    {
        //        parsedJwt = _client.NewJwtParser()
        //            .SetSigningKey(_client.Configuration.Client.ApiKey.Secret, Encoding.UTF8)
        //            .Parse(jwt);
        //        return true;
        //    }
        //    catch (InvalidJwtException)
        //    {
        //        return false;
        //    }
        //}

        //private static void AddByClaim(List<string> target, IJwt jwt, string claimName)
        //{
        //    if (!jwt.Body.ContainsClaim(claimName))
        //    {
        //        return;
        //    }

        //    var value = jwt.Body.GetClaim(claimName).ToString();
        //    if (!string.IsNullOrEmpty(value))
        //    {
        //        target.Add(value);
        //    }
        //}

        public TokenRevoker AddToken(string jwt)
        {
            // rewrite revocation story
            throw new Exception("TODO");

            //IJwt parsedJwt = null;

            //if (!IsValidJwt(jwt, out parsedJwt))
            //{
            //    return this;
            //}

            //object rawTokenType = null;
            //parsedJwt.Header.TryGetValue("stt", out rawTokenType);
            //var tokenType = rawTokenType?.ToString();

            //if (string.IsNullOrEmpty(tokenType))
            //{
            //    // Assume it's an access token
            //    AddByClaim(_accessTokenIdsToDelete, parsedJwt, "jti");
            //    return this;
            //}

            //if (tokenType.Equals("access", StringComparison.Ordinal))
            //{
            //    AddByClaim(_accessTokenIdsToDelete, parsedJwt, "jti");

            //    // Add the accompanying refresh token, if it exists
            //    AddByClaim(_refreshTokenIdsToDelete, parsedJwt, "rti");
            //}

            //if (tokenType.Equals("refresh", StringComparison.Ordinal))
            //{
            //    AddByClaim(_refreshTokenIdsToDelete, parsedJwt, "jti");
            //}

            //return this;
        }

        public Task Revoke(CancellationToken cancellationToken)
        {
            // rewrite revocation story
            throw new Exception("TODO");

            //var deleteTasks = 
            //    _accessTokenIdsToDelete
            //        .Distinct(StringComparer.Ordinal)
            //        .Select(id => DeleteAccessToken(id, cancellationToken))
            //.Concat(
            //    _refreshTokenIdsToDelete
            //        .Distinct(StringComparer.Ordinal)
            //        .Select(id => DeleteRefreshToken(id, cancellationToken)));

            //return Task.WhenAll(deleteTasks);
        }

        //private async Task<bool> DeleteAccessToken(string jti, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var accessTokenResource = await _client.GetAccessTokenAsync($"/accessTokens/{jti}", cancellationToken);
        //        return await accessTokenResource.DeleteAsync(cancellationToken);
        //    }
        //    catch (ResourceException rex)
        //    {
        //        _logger.LogInformation(rex.DeveloperMessage, source: nameof(DeleteAccessToken));
        //        return false;
        //    }
        //}

        //private async Task<bool> DeleteRefreshToken(string jti, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var refreshTokenResource = await _client.GetAccessTokenAsync($"/refreshTokens/{jti}", cancellationToken);
        //        return await refreshTokenResource.DeleteAsync(cancellationToken);
        //    }
        //    catch (ResourceException rex)
        //    {
        //        _logger.LogInformation(rex.DeveloperMessage, source: nameof(DeleteRefreshToken));
        //        return false;
        //    }
        //}
    }
}
