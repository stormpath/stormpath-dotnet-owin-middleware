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
    public class PreRegistrationHandlerShould
    {
        [Fact]
        public async Task AccessAccount()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PreRegistrationHandler = (ctx, ct) =>
                    {
                        ctx.Account.SetMiddleName("the");
                        return Task.FromResult(0);
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);

                // Act
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";
                var payload = new
                {
                    email,
                    password = "Changeme123!!",
                    givenName = "Chewbacca",
                    surname = "Wookiee"
                };

                var response = await server.PostAsync("/register", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                var account = await application.GetAccounts().Where(a => a.Email == email).SingleAsync();
                cleanup.MarkForDeletion(account);

                // Assert
                account.FullName.Should().Be("Chewbacca the Wookiee");
            }
        }
    }
}