using System;
using System.Collections.Generic;
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

            var status = string.Empty;
            try
            {
                var parsedJwt = client.NewJwtParser()
                    .SetSigningKey(_configuration.Client.ApiKey.Secret, Encoding.UTF8)
                    .Parse(stormpathToken);

                status = parsedJwt.Body.GetClaim("status").ToString();
            }
            catch (JwtSignatureException jse)
            {
                _logger.Error(jse, message: "JWT failed validation", source: nameof(StormpathCallbackRoute));
                throw; // json response
            }

            return await HandleActionAsync(context, client, application, stormpathToken, queryString, status, cancellationToken);
        }

        private Task<bool> HandleActionAsync(
            IOwinEnvironment context,
            IClient client,
            IApplication application,
            string token,
            IDictionary<string, string[]> queryString,
            string status,
            CancellationToken cancellationToken)
        {
            //if (status.Equals("registered", StringComparison.OrdinalIgnoreCase))
            //{
            //    // TODO register logic
            //    throw new NotImplementedException();
            //}

            //if (status.Equals("authenticated", StringComparison.OrdinalIgnoreCase))
            //{
            //    IOauthGrantAuthenticationResult grantResult;
            //    try
            //    {
            //        var tokenExchangeAttempt = OauthRequests.NewIdSiteTokenAuthenticationRequest()
            //            .SetJwt(token)
            //            .Build();

            //        grantResult = await application.NewIdSiteTokenAuthenticator()
            //            .AuthenticateAsync(tokenExchangeAttempt, cancellationToken);
            //    }
            //    catch (ResourceException rex)
            //    {
            //        _logger.Warn(rex, source: nameof(StormpathCallbackRoute));
            //        throw; // json response
            //    }

            //    var postLoginExecutor = new LoginExecutor(client, _configuration, _logger);

            //    await postLoginExecutor.HandlePostLoginAsync(context, grantResult, cancellationToken);
            //    await postLoginExecutor.HandleRedirectAsync(context, queryString);
            //    return true;
            //}

            //if (status.Equals("logout", StringComparison.OrdinalIgnoreCase))
            //{
            //    var logoutExecutor = new PostLogoutExecutor(client, _configuration, _logger);

            //    await logoutExecutor.HandleLogoutAsync(context, client, cancellationToken);
            //    await logoutExecutor.HandleRedirectAsync(context);
            //    return true;
            //}

            // json response: 'Unknown ID site result status: ' + status
            throw new ArgumentException();
        }
    }

}
