using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class StormpathCallbackRoute : AbstractRoute
    {
        protected override Task<bool> GetAsync(IOwinEnvironment context, ContentNegotiationResult contentNegotiationResult,
            CancellationToken cancellationToken)
        {
            // todo re-enable the /stormpathCallback route
            throw new Exception("TODO");

            //var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            //var stormpathToken = queryString.GetString("jwtResponse");

            //if (string.IsNullOrEmpty(stormpathToken))
            //{
            //    throw new ArgumentNullException(nameof(stormpathToken), "Token was null."); // TODO json response, for now
            //}

            //// TODO: Use StormpathAssertionAuthenticator at SDK level (when it's ready) to locally validate token

            //try
            //{
            //    var parsedJwt = client.NewJwtParser()
            //        .SetSigningKey(_configuration.Client.ApiKey.Secret, Encoding.UTF8)
            //        .Parse(stormpathToken);

            //    object tokenType;
            //    parsedJwt.Header.TryGetValue("stt", out tokenType);
            //    if (tokenType == null || !tokenType.ToString().Equals("assertion"))
            //    {
            //        throw new InvalidJwtException("The token is not of the correct type");
            //    }

            //    // Verify state token for authenticity
            //    string stateToken = null;
            //    if (parsedJwt.Body.ContainsClaim("state"))
            //    {
            //        stateToken = parsedJwt.Body.GetClaim("state").ToString();
            //    }

            //    var parsedStateToken = new StateTokenParser(client, _configuration.Client.ApiKey, stateToken, _logger);
            //    if (!parsedStateToken.Valid)
            //    {
            //        // Note: IsNullOrEmpty is considered invalid automatically
            //        _logger.LogWarning("State token was invalid", nameof(StormpathCallbackRoute));
            //        throw new InvalidOperationException("State token was invalid"); // TODO json response, for now
            //    }

            //    return await HandleCallbackAsync(context, client, application, parsedJwt, parsedStateToken.Path, cancellationToken);
            //}
            //catch (InvalidJwtException ije)
            //{
            //    _logger.LogError(ije, message: "JWT failed validation", source: nameof(StormpathCallbackRoute));
            //    throw; // TODO json response
            //}
        }

        private Task<bool> HandleCallbackAsync(
            IOwinEnvironment context,
            dynamic jwt,
            string nextPath,
            CancellationToken cancellationToken)
        {
            // todo re-enable the /stormpathCallback route
            throw new Exception("TODO");

            //var isNewSubscriber = false;
            //if (jwt.Body.ContainsClaim("isNewSub"))
            //{
            //    isNewSubscriber = (bool) jwt.Body.GetClaim("isNewSub");
            //}

            //var status = jwt.Body.GetClaim("status").ToString();

            //var isLogin = status.Equals("authenticated", StringComparison.OrdinalIgnoreCase);
            //var isLogout = status.Equals("logout", StringComparison.OrdinalIgnoreCase);
            //var isRegistration = isNewSubscriber || status.Equals("registered", StringComparison.OrdinalIgnoreCase);

            //if (isRegistration)
            //{
            //    var grantResult = await ExchangeTokenAsync(application, jwt, cancellationToken);

            //    var registrationExecutor = new RegisterExecutor(client, _configuration, _handlers, _logger);
            //    var account = await (await grantResult.GetAccessTokenAsync(cancellationToken)).GetAccountAsync(cancellationToken);
            //    await registrationExecutor.HandlePostRegistrationAsync(context, account, cancellationToken);

            //    return await LoginAndRedirectAsync(
            //        context,
            //        client,
            //        grantResult,
            //        true,
            //        nextPath,
            //        cancellationToken);
            //}

            //if (isLogin)
            //{
            //    var grantResult = await ExchangeTokenAsync(application, jwt, cancellationToken);

            //    return await LoginAndRedirectAsync(
            //        context,
            //        client,
            //        grantResult,
            //        false,
            //        nextPath,
            //        cancellationToken);
            //}

            //if (isLogout)
            //{
            //    var executor = new LogoutExecutor(client, _configuration, _handlers, _logger);

            //    await executor.HandleLogoutAsync(context, cancellationToken);
            //    await executor.HandleRedirectAsync(context);
            //    return true;
            //}

            //// json response
            //throw new ArgumentException($"Unknown assertion status '{status}'");
        }

        private async Task<bool> LoginAndRedirectAsync(
            IOwinEnvironment context,
            GrantResult grantResult,
            bool isNewAccount,
            string nextPath,
            CancellationToken cancellationToken)
        {
            var executor = new LoginExecutor(_configuration, _handlers, _logger);
            await executor.HandlePostLoginAsync(context, grantResult, cancellationToken);

            var defaultNextPath = isNewAccount
                ? _configuration.Web.Register.NextUri
                : _configuration.Web.Login.NextUri;

            return await executor.HandleRedirectAsync(context, nextPath, defaultNextPath);
        }

        // todo re-enable the /stormpathCallback route
        //private async Task<GrantResult> ExchangeTokenAsync(IJwt jwt, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var tokenExchangeAttempt = OauthRequests.NewIdSiteTokenAuthenticationRequest()
        //            .SetJwt(jwt.ToString())
        //            .Build();

        //        var grantResult = await application.NewIdSiteTokenAuthenticator()
        //            .AuthenticateAsync(tokenExchangeAttempt, cancellationToken);

        //        return grantResult;
        //    }
        //    catch (ResourceException rex)
        //    {
        //        _logger.LogWarning(rex, source: nameof(ExchangeTokenAsync));
        //        throw; // json response
        //    }
        //}
    }
}
