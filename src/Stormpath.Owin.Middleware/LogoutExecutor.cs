using System;
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

        public async Task HandleLogoutAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var account = context.Request[OwinKeys.StormpathUser] as IAccount;
            context.Request[OwinKeys.StormpathUser] = null;

            string[] rawCookies;
            context.Request.Headers.TryGetValue("Cookie", out rawCookies);
            var cookieParser = new CookieParser(rawCookies, _logger);

            if (account != null)
            {
                var preLogoutContext = new PreLogoutContext(context, account);
                await _handlers.PreLogoutHandler(preLogoutContext, cancellationToken);

                // TODO delete tokens for other types of auth too
                await RevokeCookieTokens(_client, cookieParser, cancellationToken);
                await RevokeHeaderToken(context, _client, cancellationToken);

                var postLogoutContext = new PostLogoutContext(context, account);
                await _handlers.PostLogoutHandler(postLogoutContext, cancellationToken);
            }

            DeleteCookies(context, cookieParser);
        }

        public Task<bool> HandleRedirectAsync(IOwinEnvironment context)
            => HttpResponse.Redirect(context, _configuration.Web.Logout.NextUri);

        private async Task RevokeCookieTokens(IClient client, CookieParser cookieParser, CancellationToken cancellationToken)
        {
            var accessToken = cookieParser.Get(_configuration.Web.AccessTokenCookie.Name);
            var refreshToken = cookieParser.Get(_configuration.Web.RefreshTokenCookie.Name);

            var revoker = new TokenRevoker(client, _logger)
                .AddToken(accessToken)
                .AddToken(refreshToken);

            try
            {
                await revoker.Revoke(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message, source: nameof(RevokeCookieTokens));
            }
        }

        private async Task RevokeHeaderToken(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var bearerHeaderParser = new BearerAuthenticationParser(context.Request.Headers.GetString("Authorization"), _logger);
            if (!bearerHeaderParser.IsValid)
            {
                return;
            }

            var revoker = new TokenRevoker(client, _logger)
                .AddToken(bearerHeaderParser.Token);

            try
            {
                await revoker.Revoke(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.Message, source: nameof(RevokeCookieTokens));
            }
        }

        private void DeleteCookies(IOwinEnvironment context, CookieParser cookieParser)
        {
            Cookies.DeleteTokenCookie(context, _configuration.Web.AccessTokenCookie, _logger);
            Cookies.DeleteTokenCookie(context, _configuration.Web.RefreshTokenCookie, _logger);
        }
    }
}
