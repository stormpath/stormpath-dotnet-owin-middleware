using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Stormpath.Owin.IntegrationTest
{
    public static class Helpers
    {
        public static HttpClient CreateServer(OwinTestFixture fixture)
        {
            return new TestServer(new WebHostBuilder().Configure(fixture.Configure)).CreateClient();
        }
    }
}
