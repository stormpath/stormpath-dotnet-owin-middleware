using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Http;
using Stormpath.SDK.Resource;
using Stormpath.SDK.Serialization;
using Stormpath.SDK.Sync;

namespace Stormpath.Owin.IntegrationTest
{
    public class TestEnvironment : IDisposable
    {
        private readonly IClient _client;
        private readonly List<IDeletable> _cleanupList;
        private readonly Action<string> _log;

        public TestEnvironment(IClient client, Func<IClient, Task<IDeletable[]>> setupAction, Action<string> logAction = null)
        {
            if (logAction == null)
            {
                logAction = _ => { };
            }
            _log = logAction;

            _client = client;
            _cleanupList = new List<IDeletable>();

            _client.GetCurrentTenant();
            var resources = setupAction(_client).Result;
            _cleanupList.AddRange(resources);
        }

        public void MarkForDeletion(IDeletable resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            _cleanupList.Add(resource);
        }

        public void Dispose()
        {
            foreach (var resource in (_cleanupList as IEnumerable<IDeletable>).Reverse())
            {
                try
                {
                    resource.Delete();
                }
                catch (ResourceException rex)
                {
                    _log($"Could not delete {(resource as IResource)?.Href} - '{rex.DeveloperMessage}'");
                }
            }
        }
    }
}
