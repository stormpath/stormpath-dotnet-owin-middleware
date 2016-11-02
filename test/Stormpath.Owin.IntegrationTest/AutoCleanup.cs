using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Resource;
using Stormpath.SDK.Sync;

namespace Stormpath.Owin.IntegrationTest
{
    public class AutoCleanup : IDisposable
    {
        private readonly List<IDeletable> _cleanupList;
        private readonly Action<string> _log;
        private bool _disposed;

        public AutoCleanup(IClient client, Func<IClient, Task<IResource[]>> setupAction = null, Action<string> logAction = null)
        {
            if (logAction == null)
            {
                logAction = _ => { };
            }
            _log = logAction;

            _cleanupList = new List<IDeletable>();

            client.GetCurrentTenant();

            if (setupAction == null)
            {
                setupAction = _ => Task.FromResult(new IResource[] {});
            }

            var resources = setupAction(client).Result;
            _cleanupList.AddRange(resources.OfType<IDeletable>());
        }

        public void MarkForDeletion(IDeletable resource)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AutoCleanup), "The environment has already been cleaned up");
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            _cleanupList.Add(resource);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AutoCleanup), "The environment has already been cleaned up");
            }

            var exceptions = new List<Exception>();

            foreach (var resource in (_cleanupList as IEnumerable<IDeletable>).Reverse())
            {
                try
                {
                    resource.Delete();
                }
                catch (ResourceException rex)
                {
                    _log($"Could not delete {(resource as IResource)?.Href} - '{rex.DeveloperMessage}'");
                    exceptions.Add(rex);
                }
                catch (Exception ex)
                {
                    _log($"Could not delete {(resource as IResource)?.Href} - '{ex.Message}'");
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException("Could not clean up all resources", exceptions);
            }

            _disposed = true;
        }
    }
}
