using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Application;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Jwt;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Oauth;

namespace Stormpath.Owin.Middleware.Route
{
    public class StormpathCallbackRoute : AbstractRoute
    {
        protected override async Task<bool> GetAsync(IOwinEnvironment context, IClient client, ContentNegotiationResult contentNegotiationResult,
            CancellationToken cancellationToken)
        {
            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);
            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var stormpathToken = queryString.GetString("jwtResponse");

            if (string.IsNullOrEmpty(stormpathToken))
            {
                throw new ArgumentNullException(nameof(stormpathToken), "Token was null.");
            }

            // TODO: Use StormpathAssertionAuthenticator at SDK level (when it's ready) to locally validate token

            try
            {
                var parsedJwt = client.NewJwtParser()
                    .SetSigningKey(_configuration.Client.ApiKey.Secret, Encoding.UTF8)
                    .Parse(stormpathToken);

                object tokenType;
                parsedJwt.Header.TryGetValue("stt", out tokenType);
                if (tokenType == null || !tokenType.ToString().Equals("assertion"))
                {
                    throw new InvalidJwtException("The token is not of the correct type");
                }

                return await HandleCallbackAsync(context, client, application, parsedJwt, cancellationToken);
            }
            catch (InvalidJwtException ije)
            {
                _logger.Error(ije, message: "JWT failed validation", source: nameof(StormpathCallbackRoute));
                throw; // json response
            }
        }

        private async Task<bool> HandleCallbackAsync(
            IOwinEnvironment context,
            IClient client,
            IApplication application,
            IJwt jwt,
            CancellationToken cancellationToken)
        {
            var isNewSubscriber = false;
            if (jwt.Body.ContainsClaim("isNewSub"))
            {
                isNewSubscriber = (bool) jwt.Body.GetClaim("isNewSub");
            }

            var status = jwt.Body.GetClaim("status").ToString();

            var isLogin = status.Equals("authenticated", StringComparison.OrdinalIgnoreCase);
            var isLogout = status.Equals("logout", StringComparison.OrdinalIgnoreCase);
            var isRegistration = isNewSubscriber || status.Equals("registered", StringComparison.OrdinalIgnoreCase);

            if (isRegistration)
            {
                var grantResult = await ExchangeTokenAsync(application, jwt, cancellationToken);

                var registrationExecutor = new RegisterExecutor(client, _configuration, _handlers, _logger);
                var account = await (await grantResult.GetAccessTokenAsync(cancellationToken)).GetAccountAsync(cancellationToken);
                await registrationExecutor.HandlePostRegistrationAsync(context, account, cancellationToken);

                var loginExecutor = new LoginExecutor(client, _configuration, _handlers, _logger);
                await loginExecutor.HandlePostLoginAsync(context, grantResult, cancellationToken);
                await loginExecutor.HandleRedirectAsync(context); // TODO: support deep link redirection

                return true;
            }

            if (isLogin)
            {
                var grantResult = await ExchangeTokenAsync(application, jwt, cancellationToken);

                var executor = new LoginExecutor(client, _configuration, _handlers, _logger);

                await executor.HandlePostLoginAsync(context, grantResult, cancellationToken);
                await executor.HandleRedirectAsync(context); // TODO: support deep link redirection

                return true;
            }

            if (isLogout)
            {
                var executor = new LogoutExecutor(client, _configuration, _handlers, _logger);

                await executor.HandleLogoutAsync(context, cancellationToken);
                await executor.HandleRedirectAsync(context);
                return true;
            }

            // json response: 'Unknown ID site result status: ' + status
            throw new ArgumentException($"Unknown assertion status '{status}'");
        }

        private async Task<IOauthGrantAuthenticationResult> ExchangeTokenAsync(IApplication application, IJwt jwt, CancellationToken cancellationToken)
        {
            try
            {
                var tokenExchangeAttempt = OauthRequests.NewIdSiteTokenAuthenticationRequest()
                    .SetJwt(jwt.ToString())
                    .Build();

                var grantResult = await application.NewIdSiteTokenAuthenticator()
                    .AuthenticateAsync(tokenExchangeAttempt, cancellationToken);

                return grantResult;
            }
            catch (ResourceException rex)
            {
                _logger.Warn(rex, source: nameof(ExchangeTokenAsync));
                throw; // json response
            }
        }
    }
}
