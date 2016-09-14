using System;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;

namespace Stormpath.Owin.Middleware
{
    public static class DefaultHandlers
    {
        public static Func<PreChangePasswordContext, CancellationToken, Task> PreChangePasswordHandler
            = (ctx, ct) => TaskConstants.CompletedTask;

        public static Func<PostChangePasswordContext, CancellationToken, Task> PostChangePasswordHandler
            = (ctx, ct) => TaskConstants.CompletedTask;

        public static Func<PreLoginContext, CancellationToken, Task> PreLoginHandler
            = (ctx, ct) => TaskConstants.CompletedTask;

        public static Func<PostLoginContext, CancellationToken, Task> PostLoginHandler
            = (ctx, ct) => TaskConstants.CompletedTask;

        public static Func<PreLogoutContext, CancellationToken, Task> PreLogoutHandler
            = (ctx, ct) => TaskConstants.CompletedTask;

        public static Func<PostLogoutContext, CancellationToken, Task> PostLogoutHandler
            = (ctx, ct) => TaskConstants.CompletedTask;

        public static Func<PreRegistrationContext, CancellationToken, Task> PreRegistrationHandler
            = (ctx, ct) => TaskConstants.CompletedTask;

        public static Func<PostRegistrationContext, CancellationToken, Task> PostRegistrationHandler
            = (ctx, ct) => TaskConstants.CompletedTask;

        public static Func<PreVerifyEmailContext, CancellationToken, Task> PreVerifyEmailHandler
            = (ctx, ct) => TaskConstants.CompletedTask;

        public static Func<PostVerifyEmailContext, CancellationToken, Task> PostVerifyEmailHandler
            = (ctx, ct) => TaskConstants.CompletedTask;
    }
}
