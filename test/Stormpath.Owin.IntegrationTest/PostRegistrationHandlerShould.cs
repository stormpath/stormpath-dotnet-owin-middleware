using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Stormpath.Owin.Middleware;
using Stormpath.SDK;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    public class PostRegistrationHandlerShould
    {
        [Fact]
        public async Task AccessAccountAfterRegistration()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PostRegistrationHandler = async (ctx, ct) =>
                    {
                        ctx.Account.CustomData["homeworld"] = "Alderaan";
                        await ctx.Account.SaveAsync();
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);

                // Act
                var email = $"its-{fixture.TestKey}@example.com";
                var payload = new
                {
                    email,
                    password = "Changeme123!!",
                    givenName = "Princess",
                    surname = "Leia"
                };

                var response = await server.PostAsync("/register", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                var account = await application.GetAccounts().Where(a => a.Email == email).SingleAsync();
                cleanup.MarkForDeletion(account);

                // Assert
                var customData = await account.GetCustomDataAsync();
                customData["homeworld"].Should().Be("Alderaan");
            }
        }
    }
}