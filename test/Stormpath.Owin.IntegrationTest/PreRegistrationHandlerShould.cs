using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        [Fact]
        public async Task ReturnDefaultErrorMessageDuringFormPost()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PreRegistrationHandler = (ctx, ct) =>
                    {
                        ctx.Result = new PreRegistrationResult()
                        {
                            Success = false
                        };
                        return Task.FromResult(0);
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);
            var csrfToken = await CsrfToken.GetTokenForRoute(server, "/register");

            // Act
            var payload = new Dictionary<string, string>()
            {
                ["email"] = $"its-{fixture.TestKey}@testmail.stormpath.com",
                ["password"] = "Changeme123!!",
                ["givenName"] = "Jyn",
                ["surname"] = "Erso",
                ["st"] = csrfToken,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/register")
            {
                Content = new FormUrlEncodedContent(payload)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

            var response = await server.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("An error has occurred. Please try again.");
        }

        [Fact]
        public async Task ReturnDefaultErrorMessageDuringJsonPost()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PreRegistrationHandler = (ctx, ct) =>
                    {
                        ctx.Result = new PreRegistrationResult()
                        {
                            Success = false
                        };
                        return Task.FromResult(0);
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            // Act
            var payload = new
            {
                email = $"its-{fixture.TestKey}@testmail.stormpath.com",
                password = "Changeme123!!",
                givenName = "Jyn",
                surname = "Erso"
            };

            var response = await server.PostAsync("/register", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await response.Content.ReadAsStringAsync()).Should().Contain("An error has occurred. Please try again.");
        }

        [Fact]
        public async Task ReturnCustomErrorMessageDuringFormPost()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PreRegistrationHandler = (ctx, ct) =>
                    {
                        ctx.Result = new PreRegistrationResult()
                        {
                            Success = false,
                            ErrorMessage = "Nice try, rebel scum!"
                        };
                        return Task.FromResult(0);
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);
            var csrfToken = await CsrfToken.GetTokenForRoute(server, "/register");

            // Act
            var payload = new Dictionary<string, string>()
            {
                ["email"] = $"its-{fixture.TestKey}@testmail.stormpath.com",
                ["password"] = "Changeme123!!",
                ["givenName"] = "Jyn",
                ["surname"] = "Erso",
                ["st"] = csrfToken,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/register")
            {
                Content = new FormUrlEncodedContent(payload)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

            var response = await server.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("Nice try, rebel scum!");
        }

        [Fact]
        public async Task ReturnCustomErrorMessageDuringJsonPost()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PreRegistrationHandler = (ctx, ct) =>
                    {
                        ctx.Result = new PreRegistrationResult()
                        {
                            Success = false,
                            ErrorMessage = "Nice try, rebel scum!"
                        };
                        return Task.FromResult(0);
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            // Act
            var payload = new
            {
                email = $"its-{fixture.TestKey}@testmail.stormpath.com",
                password = "Changeme123!!",
                givenName = "Jyn",
                surname = "Erso"
            };

            var response = await server.PostAsync("/register", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await response.Content.ReadAsStringAsync()).Should().Contain("\"message\": \"Nice try, rebel scum!\"");
        }
    }
}