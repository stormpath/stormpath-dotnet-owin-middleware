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
    public class PostLoginHandlerShould
    {
        [Fact]
        public async Task AccessAccountAfterLogin()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PostLoginHandler = async (ctx, ct) =>
                    {
                        ctx.Account.CustomData["THX"] = "1138";
                        await ctx.Account.SaveAsync();
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var email = $"its-{fixture.TestKey}@example.com";
                var account = await application.CreateAccountAsync(
                    nameof(AccessAccountAfterLogin), 
                    nameof(PostLoginHandlerShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var payload = new
                {
                    login = email,
                    password = "Changeme123!!"
                };

                // Act
                var response = await server.PostAsync("/login", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                // Assert
                var customData = await account.GetCustomDataAsync();
                customData["THX"].Should().Be("1138");
            }
        }
    }
}