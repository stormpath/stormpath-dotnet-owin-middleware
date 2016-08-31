using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware
{
    public sealed class HandlerConfiguration
    {
        public HandlerConfiguration(
            Func<PreChangePasswordContext, CancellationToken, Task> preChangePasswordHandler,
            Func<PostChangePasswordContext, CancellationToken, Task> postChangePasswordHandler,
            Func<PreLoginContext, CancellationToken, Task> preLoginHandler,
            Func<PostLoginContext, CancellationToken, Task> postLoginHandler,
            Func<PreLogoutContext, CancellationToken, Task> preLogoutHandler,
            Func<PostLogoutContext, CancellationToken, Task> postLogoutHandler,
            Func<PreRegistrationContext, CancellationToken, Task> preRegistrationHandler,
            Func<PostRegistrationContext, CancellationToken, Task> postRegistrationHandler,
            Func<PreVerifyEmailContext, CancellationToken, Task> preVerifyEmailHandler,
            Func<PostVerifyEmailContext, CancellationToken, Task> postVerifyEmailHandler)
        {
            if (preChangePasswordHandler == null) throw new ArgumentNullException(nameof(preChangePasswordHandler));
            if (postChangePasswordHandler == null) throw new ArgumentNullException(nameof(postChangePasswordHandler));

            if (preLoginHandler == null) throw new ArgumentNullException(nameof(preLoginHandler));
            if (postLoginHandler == null) throw new ArgumentNullException(nameof(postLoginHandler));

            if (preLogoutHandler == null) throw new ArgumentNullException(nameof(preLogoutHandler));
            if (postLogoutHandler == null) throw new ArgumentNullException(nameof(postLogoutHandler));

            if (preRegistrationHandler == null) throw new ArgumentNullException(nameof(preRegistrationHandler));
            if (postRegistrationHandler == null) throw new ArgumentNullException(nameof(postRegistrationHandler));

            if (preVerifyEmailHandler == null) throw new ArgumentNullException(nameof(preVerifyEmailHandler));
            if (postVerifyEmailHandler == null) throw new ArgumentNullException(nameof(postVerifyEmailHandler));

            PreChangePasswordHandler = preChangePasswordHandler;
            PostChangePasswordHandler = postChangePasswordHandler;

            PreLoginHandler = preLoginHandler;
            PostLoginHandler = postLoginHandler;

            PreLogoutHandler = preLogoutHandler;
            PostLogoutHandler = postLogoutHandler;

            PreRegistrationHandler = preRegistrationHandler;
            PostRegistrationHandler = postRegistrationHandler;

            PreVerifyEmailHandler = preVerifyEmailHandler;
            PostVerifyEmailHandler = postVerifyEmailHandler;
        }

        public Func<PreChangePasswordContext, CancellationToken, Task> PreChangePasswordHandler { get; }

        public Func<PostChangePasswordContext, CancellationToken, Task> PostChangePasswordHandler { get; }

        public Func<PreLoginContext, CancellationToken, Task> PreLoginHandler { get; }

        public Func<PostLoginContext, CancellationToken, Task> PostLoginHandler { get; }

        public Func<PreLogoutContext, CancellationToken, Task> PreLogoutHandler { get; }

        public Func<PostLogoutContext, CancellationToken, Task> PostLogoutHandler { get; }

        public Func<PreRegistrationContext, CancellationToken, Task> PreRegistrationHandler { get; }

        public Func<PostRegistrationContext, CancellationToken, Task> PostRegistrationHandler { get; }

        public Func<PreVerifyEmailContext, CancellationToken, Task> PreVerifyEmailHandler { get; }

        public Func<PostVerifyEmailContext, CancellationToken, Task> PostVerifyEmailHandler { get; }
    }
}

