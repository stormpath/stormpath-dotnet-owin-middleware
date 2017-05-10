using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Okta;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class StormpathCallbackRoute : AbstractRoute
    {
        protected override async Task<bool> GetAsync(
            IOwinEnvironment context,
            ContentNegotiationResult contentNegotiationResult,
            CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var code = queryString.GetString("code");

            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException(nameof(code), "Code was null."); // TODO json response, for now
            }

            // Verify state token for authenticity
            var stateToken = queryString.GetString("state");

            var parsedStateToken = new StateTokenParser(_configuration.Application.Id, _configuration.OktaEnvironment.ClientSecret, stateToken, _logger);
            if (!parsedStateToken.Valid)
            {
                throw new InvalidOperationException("State token was invalid"); // TODO json response, for now
            }

            var grantResult = await _oktaClient.PostAuthCodeGrantAsync(
                _configuration.OktaEnvironment.AuthorizationServerId,
                _configuration.OktaEnvironment.ClientId,
                _configuration.OktaEnvironment.ClientSecret,
                code,
                _configuration.AbsoluteCallbackUri,
                cancellationToken);

            var user = await UserHelper.GetUserFromAccessTokenAsync(_oktaClient, grantResult.AccessToken, _logger, cancellationToken);

            return await LoginAndRedirectAsync(context, grantResult, user, parsedStateToken.Path, cancellationToken);
        }

        private async Task<bool> LoginAndRedirectAsync(
            IOwinEnvironment context,
            GrantResult grantResult,
            User user,
            string nextPath,
            CancellationToken cancellationToken)
        {
            var executor = new LoginExecutor(_configuration, _handlers, _oktaClient, _logger);
            await executor.HandlePostLoginAsync(context, grantResult, user, cancellationToken);

            // TODO determine whether this is a new account or not

            return await executor.HandleRedirectAsync(context, nextPath, _configuration.Web.Login.NextUri);
        }
    }
}
