using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Stormpath.Owin.Middleware;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    public class PreLoginHandlerShould
    {
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
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var account = await application.CreateAccountAsync(
                    nameof(AlterLogin),
                    nameof(PreLoginHandlerShould),
                    $"its-{fixture.TestKey}@example.com",
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var payload = new
                {
                    login = $"its-{fixture.TestKey}@example", // missing ".com"
                    password = "Changeme123!!"
                };

                // Act
                var response = await server.PostAsync("/login", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue();
            }
        }
    }
}