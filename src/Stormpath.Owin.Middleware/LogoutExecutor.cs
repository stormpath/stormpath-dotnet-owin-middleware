﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;


namespace Stormpath.Owin.Middleware
{
    internal sealed class LogoutExecutor
    {
        private readonly StormpathConfiguration _configuration;
        private readonly HandlerConfiguration _handlers;
        private readonly ILogger _logger;

        public LogoutExecutor(
            StormpathConfiguration configuration,
            HandlerConfiguration handlers,
            ILogger logger)
        {
            _configuration = configuration;
            _handlers = handlers;
            _logger = logger;
        }

        public async Task HandleLogoutAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            dynamic account = context.Request[OwinKeys.StormpathUser];
            context.Request[OwinKeys.StormpathUser] = null;

            string[] rawCookies;
            context.Request.Headers.TryGetValue("Cookie", out rawCookies);
            var cookieParser = new CookieParser(rawCookies, _logger);

            if (account != null)
            {
                var preLogoutContext = new PreLogoutContext(context, account);
                await _handlers.PreLogoutHandler(preLogoutContext, cancellationToken);

                // TODO delete tokens for other types of auth too
                await RevokeCookieTokens(cookieParser, cancellationToken);
                await RevokeHeaderToken(context, cancellationToken);

                var postLogoutContext = new PostLogoutContext(context, account);
                await _handlers.PostLogoutHandler(postLogoutContext, cancellationToken);
            }

            DeleteCookies(context, cookieParser);
        }

        public Task<bool> HandleRedirectAsync(IOwinEnvironment context)
            => HttpResponse.Redirect(context, _configuration.Web.Logout.NextUri);

        private async Task RevokeCookieTokens(CookieParser cookieParser, CancellationToken cancellationToken)
        {
            var accessToken = cookieParser.Get(_configuration.Web.AccessTokenCookie.Name);
            var refreshToken = cookieParser.Get(_configuration.Web.RefreshTokenCookie.Name);

            var revoker = new TokenRevoker(_logger)
                .AddToken(accessToken)
                .AddToken(refreshToken);

            try
            {
                await revoker.Revoke(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message, nameof(RevokeCookieTokens));
            }
        }

        private async Task RevokeHeaderToken(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            var bearerHeaderParser = new BearerAuthenticationParser(context.Request.Headers.GetString("Authorization"), _logger);
            if (!bearerHeaderParser.IsValid)
            {
                return;
            }

            var revoker = new TokenRevoker(_logger)
                .AddToken(bearerHeaderParser.Token);

            try
            {
                await revoker.Revoke(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message, nameof(RevokeCookieTokens));
            }
        }

        private void DeleteCookies(IOwinEnvironment context, CookieParser cookieParser)
        {
            Cookies.DeleteTokenCookie(context, _configuration.Web.AccessTokenCookie, _logger);
            Cookies.DeleteTokenCookie(context, _configuration.Web.RefreshTokenCookie, _logger);
        }
    }
}
