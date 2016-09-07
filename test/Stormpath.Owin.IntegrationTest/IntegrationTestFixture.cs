using System;
using System.Collections.Generic;
using Stormpath.SDK.Application;
using Stormpath.SDK.Client;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Http;
using Stormpath.SDK.Resource;
using Stormpath.SDK.Serialization;

namespace Stormpath.Owin.IntegrationTest
{
    public class IntegrationTestFixture : IDisposable
    {
        private readonly TestEnvironment _environment;

        public IntegrationTestFixture()
        {
            Client = Clients.Builder()
                .SetHttpClient(HttpClients.Create().SystemNetHttpClient())
                .SetSerializer(Serializers.Create().JsonNetSerializer())
                .Build();

            TestInstanceKey = Guid.NewGuid().ToString();

            _environment = new TestEnvironment(Client, async client =>
            {
                TestApplication = await client.CreateApplicationAsync($"Stormpath.Owin IT {TestInstanceKey}", true);
                TestDirectory = await TestApplication.GetDefaultAccountStoreAsync() as IDirectory;
                return new IDeletable[] {TestApplication, TestDirectory};
            });
        }

        public IClient Client { get; }

        public string TestInstanceKey { get; }

        public IApplication TestApplication { get; private set; }

        public IDirectory TestDirectory { get; private set; }

        public void Dispose()
        {
            _environment.Dispose();
        }
    }
}
