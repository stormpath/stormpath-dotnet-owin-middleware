using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Middleware;
using Stormpath.SDK.Resource;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    public class PreLoginHandlerShould
    {
        private static HttpClient CreateServer(OwinTestFixture fixture)
        {
            return new TestServer(new WebHostBuilder().Configure(fixture.Configure)).CreateClient();
        }

        [Fact]
        public async Task AlterLogin()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PreLoginHandler = (ctx, ct) =>
                    {
                        ctx.Login = ctx.Login + ".com";
                        return Task.FromResult(0);
                    }
                }
            };
            var server = CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var account = await application.CreateAccountAsync("PreLoginHandlerShould", "AlterLogin", $"its-{fixture.TestKey}@example.com", "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var payload = new Dictionary<string, string>()
                {
                    ["login"] = $"its-{fixture.TestKey}@example", // missing ".com"
                    ["password"] = "Changeme123!!"
                };

                // Act
                var response = await server.PostAsync("/login", new FormUrlEncodedContent(payload));

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue();
            }
        }
    }
}