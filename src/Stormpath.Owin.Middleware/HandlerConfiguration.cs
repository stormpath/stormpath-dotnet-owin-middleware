using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware
{
    public sealed class HandlerConfiguration
    {
        public HandlerConfiguration(
            Func<PreRegistrationContext, CancellationToken, Task> preRegistrationHandler,
            Func<PostRegistrationContext, CancellationToken, Task> postRegistrationHandler)
        {
            if (preRegistrationHandler == null)
            {
                throw new ArgumentNullException(nameof(preRegistrationHandler));
            }

            if (postRegistrationHandler == null)
            {
                throw new ArgumentNullException(nameof(postRegistrationHandler));
            }

            PreRegistrationHandler = preRegistrationHandler;
            PostRegistrationHandler = postRegistrationHandler;
        }

        public Func<PreRegistrationContext, CancellationToken, Task> PreRegistrationHandler { get; }

        public Func<PostRegistrationContext, CancellationToken, Task> PostRegistrationHandler { get; }
    }
}
