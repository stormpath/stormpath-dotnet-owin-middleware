using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Stormpath.Owin.Middleware;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    public class PreLogoutHandlerShould
    {
        [Fact]
        public async Task AccessAccount()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PreLogoutHandler = async (ctx, ct) =>
                    {
                        ctx.Account.CustomData["micdrop"] = true;
                        await ctx.Account.SaveAsync();
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var account = await application.CreateAccountAsync(
                    nameof(AccessAccount),
                    nameof(PreLogoutHandlerShould),
                    $"its-{fixture.TestKey}@example.com",
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var payload = new
                {
                    login = $"its-{fixture.TestKey}@example.com",
                    password = "Changeme123!!"
                };

                var loginResponse = await server.PostAsync("/login", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
                loginResponse.EnsureSuccessStatusCode();

                var accessTokenCookie = loginResponse.Headers.GetValues("Set-Cookie")
                    .First(h => h.StartsWith("access_token="));
                var accessToken = accessTokenCookie.Split(';')[0].Replace("access_token=", string.Empty);

                // Act
                var request = new HttpRequestMessage(HttpMethod.Post, "/logout");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);

                var response = await server.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Assert
                var customData = await account.GetCustomDataAsync();
                customData["micdrop"].Should().Be(true);
            }
        }
    }
}