using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Resource;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    public class LogoutRouteShould
    {

        [Fact]
        public async Task DeleteCookiesProperly()
        {
            // Arrange
            var fixture = new OwinTestFixture();
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                // Create a user
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var email = $"its-{fixture.TestKey}@example.com";
                var account = await application.CreateAccountAsync(
                    nameof(DeleteCookiesProperly),
                    nameof(LogoutRouteShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                // Get a token
                var payload = new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = email,
                    ["password"] = "Changeme123!!"
                };

                var tokenResponse = await server.PostAsync("/oauth/token", new FormUrlEncodedContent(payload));
                tokenResponse.EnsureSuccessStatusCode();

                var tokenResponseContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(await tokenResponse.Content.ReadAsStringAsync());
                var accessToken = tokenResponseContent["access_token"];
                var refreshToken = tokenResponseContent["refresh_token"];

                // Create a logout request
                var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/logout");
                logoutRequest.Headers.Add("Cookie", $"access_token={accessToken}");
                logoutRequest.Headers.Add("Cookie", $"refresh_token={refreshToken}");
                logoutRequest.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);

                // Act
                var logoutResponse = await server.SendAsync(logoutRequest);
                logoutResponse.EnsureSuccessStatusCode();

                // Assert
                var setCookieHeaders = logoutResponse.Headers.GetValues("Set-Cookie").ToArray();
                setCookieHeaders.Length.Should().Be(2);
                setCookieHeaders.Should().Contain("access_token=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; HttpOnly");
                setCookieHeaders.Should().Contain("refresh_token=; path=/; expires=Thu, 01-Jan-1970 00:00:00 GMT; HttpOnly");
            }
        }
    }
}
