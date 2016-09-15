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
    public class PreChangePasswordHandlerShould
    {
        [Fact]
        public async Task AccessAccount()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PreChangePasswordHandler = async (ctx, ct) =>
                    {
                        ctx.Account.CustomData["favoriteDroid"] = "R2-D2";
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
                    nameof(AccessAccount), 
                    nameof(PreChangePasswordHandlerShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var token = await application.SendPasswordResetEmailAsync(email);

                var payload = new
                {
                    sptoken = token.GetValue(),
                    password = "Changeme456$$"
                };

                // Act
                var response = await server.PostAsync("/change", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                // Assert
                var customData = await account.GetCustomDataAsync();
                customData["favoriteDroid"].Should().Be("R2-D2");
            }
        }
    }
}