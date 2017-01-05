using System;
using System.Collections.Generic;
using System.Net;
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
    public class PostLoginHandlerShould
    {
        [Fact]
        public async Task AccessAccount()
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
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";
                var account = await application.CreateAccountAsync(
                    nameof(AccessAccount), 
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

        [Fact]
        public async Task RedirectToCustomUri()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PostLoginHandler = (ctx, ct) =>
                    {
                        ctx.Result = new PostLoginResult
                        {
                            RedirectUri = "/foobar"
                        };
                        return Task.FromResult(true);
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);
            var csrfToken = await CsrfToken.GetTokenForRoute(server, "/login");

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";
                var account = await application.CreateAccountAsync(
                    nameof(RedirectToCustomUri),
                    nameof(PostLoginHandlerShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var payload = new Dictionary<string, string>()
                {
                    ["login"] = email,
                    ["password"] = "Changeme123!!",
                    ["st"] = csrfToken,
                };

                // Act
                var request = new HttpRequestMessage(HttpMethod.Post, "/login")
                {
                    Content = new FormUrlEncodedContent(payload)
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                var response = await server.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
                response.Headers.Location.ToString().Should().Be("/foobar");
            }
        }

        [Fact]
        public async Task RedirectToDeepLinkUriViaStateToken()
        {
            // Arrange
            var fixture = new OwinTestFixture();
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";
                var account = await application.CreateAccountAsync(
                    nameof(RedirectToCustomUri),
                    nameof(PostLoginHandlerShould),
                    email,
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var stateToken = fixture.Client.NewJwtBuilder()
                    .SetClaims(new Dictionary<string, object>()
                    {
                        ["state"] = Guid.NewGuid().ToString(),
                        ["path"] = "/zomg"
                    })
                    .SetExpiration(DateTimeOffset.UtcNow.AddMinutes(1))
                    .SignWith(fixture.Client.Configuration.Client.ApiKey.Secret, Encoding.UTF8)
                    .Build()
                    .ToString();

                var payload = new Dictionary<string, string>()
                {
                    ["login"] = email,
                    ["password"] = "Changeme123!!",
                    ["st"] = stateToken,
                };

                // Act
                var request = new HttpRequestMessage(HttpMethod.Post, "/login")
                {
                    Content = new FormUrlEncodedContent(payload)
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                var response = await server.SendAsync(request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
                response.Headers.Location.ToString().Should().Be("/zomg");
            }
        }
    }
}