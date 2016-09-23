using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Jwt;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware
{
    internal sealed class LogoutExecutor
    {
        private readonly IClient _client;
        private readonly StormpathConfiguration _configuration;
        private readonly HandlerConfiguration _handlers;
        private readonly ILogger _logger;

        public LogoutExecutor(
            IClient client,
            StormpathConfiguration configuration,
            HandlerConfiguration handlers,
            ILogger logger)
        {
            _client = client;
            _configuration = configuration;
            _handlers = handlers;
            _logger = logger;
        }

        public async Task HandleLogout(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var account = context.Request[OwinKeys.StormpathUser] as IAccount;

            var preLogoutContext = new PreLogoutContext(context, account);
            await _handlers.PreLogoutHandler(preLogoutContext, cancellationToken);

            // Remove user from request
            context.Request[OwinKeys.StormpathUser] = null;

            string[] rawCookies;
            context.Request.Headers.TryGetValue("Cookie", out rawCookies);
            var cookieParser = new CookieParser(rawCookies, _logger);

            await RevokeTokens(_client, cookieParser, cancellationToken);

            // TODO delete tokens for other types of auth too
            DeleteCookies(context, cookieParser);

            var postLogoutContext = new PostLogoutContext(context, account);
            await _handlers.PostLogoutHandler(postLogoutContext, cancellationToken);
        }

        public Task<bool> HandleRedirectAsync(IOwinEnvironment context)
            => HttpResponse.Redirect(context, _configuration.Web.Logout.NextUri);

        private async Task RevokeTokens(IClient client, CookieParser cookieParser, CancellationToken cancellationToken)
        {
            var accessToken = cookieParser.Get(_configuration.Web.AccessTokenCookie.Name);
            var refreshToken = cookieParser.Get(_configuration.Web.RefreshTokenCookie.Name);

            var deleteAccessTokenTask = Task.FromResult(false);
            var deleteRefreshTokenTask = Task.FromResult(false);

            string jti;
            if (IsValidJwt(accessToken, client, out jti))
            {
                try
                {
                    var accessTokenResource = await client.GetAccessTokenAsync($"/accessTokens/{jti}", cancellationToken);
                    deleteAccessTokenTask = accessTokenResource.DeleteAsync(cancellationToken);
                }
                catch (ResourceException rex)
                {
                    _logger.Info(rex.DeveloperMessage, source: nameof(RevokeTokens));
                }
            }

            if (IsValidJwt(refreshToken, client, out jti))
            {
                try
                {
                    var refreshTokenResource = await client.GetRefreshTokenAsync($"/refreshTokens/{jti}", cancellationToken);
                    deleteRefreshTokenTask = refreshTokenResource.DeleteAsync(cancellationToken);
                }
                catch (ResourceException rex)
                {
                    _logger.Info(rex.DeveloperMessage, source: nameof(RevokeTokens));
                }
            }

            try
            {
                await Task.WhenAll(deleteAccessTokenTask, deleteRefreshTokenTask);
            }
            catch (ResourceException rex)
            {
                _logger.Info(rex.DeveloperMessage, source: nameof(RevokeTokens));
            }
        }

        private static bool IsValidJwt(string jwt, IClient client, out string jti)
        {
            jti = null;

            if (string.IsNullOrEmpty(jwt))
            {
                return false;
            }

            try
            {
                var parsed = client.NewJwtParser()
                    .SetSigningKey(client.Configuration.Client.ApiKey.Secret, Encoding.UTF8)
                    .Parse(jwt);
                jti = parsed.Body.Id;
                return true;
            }
            catch (InvalidJwtException)
            {
                return false;
            }
        }

        private void DeleteCookies(IOwinEnvironment context, CookieParser cookieParser)
        {
            if (cookieParser.Contains(_configuration.Web.AccessTokenCookie.Name))
            {
                Cookies.DeleteTokenCookie(context, _configuration.Web.AccessTokenCookie, _logger);
            }
            if (cookieParser.Contains(_configuration.Web.RefreshTokenCookie.Name))
            {
                Cookies.DeleteTokenCookie(context, _configuration.Web.RefreshTokenCookie, _logger);
            }
        }
    }
}
