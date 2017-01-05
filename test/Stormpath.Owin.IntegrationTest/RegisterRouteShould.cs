using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Middleware;
using Stormpath.SDK;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    public class RegisterRouteShould
    {
        [Fact]
        public async Task RedirectToLogin()
        {
            // Arrange
            var fixture = new OwinTestFixture(); // all default settings
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var csrfToken = await CsrfToken.GetTokenForRoute(server, "/register");
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";

                var payload = new Dictionary<string, string>()
                {
                    ["email"] = email,
                    ["password"] = "Changeme123!!",
                    ["givenName"] = nameof(RedirectToLogin),
                    ["surname"] = nameof(RegisterRouteShould),
                    ["st"] = csrfToken,
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "/register")
                {
                    Content = new FormUrlEncodedContent(payload)
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                // Act
                var response = await server.SendAsync(request);

                var foundAccount = await application.GetAccounts().Where(a => a.Email == email).SingleOrDefaultAsync();
                if (foundAccount != null)
                {
                    cleanup.MarkForDeletion(foundAccount);
                }
                
                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
                response.Headers.Location.ToString().Should().StartWith("/login?status=created");
            }
        }

        [Fact]
        public async Task RedirectToNextUriIfAutologinIsEnabled()
        {
            // Arrange
            var config = new StormpathConfiguration()
            {
                Web = new WebConfiguration()
                {
                    Register = new WebRegisterRouteConfiguration()
                    {
                        AutoLogin = true,
                        // default NextUri
                    }
                }
            };
            var fixture = new OwinTestFixture()
            {
                Options = new StormpathOwinOptions()
                {
                    Configuration = config
                }
            };
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var csrfToken = await CsrfToken.GetTokenForRoute(server, "/register");
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";

                var payload = new Dictionary<string, string>()
                {
                    ["email"] = email,
                    ["password"] = "Changeme123!!",
                    ["givenName"] = nameof(RedirectToLogin),
                    ["surname"] = nameof(RegisterRouteShould),
                    ["st"] = csrfToken,
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "/register")
                {
                    Content = new FormUrlEncodedContent(payload)
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                // Act
                var response = await server.SendAsync(request);

                var foundAccount = await application.GetAccounts().Where(a => a.Email == email).SingleOrDefaultAsync();
                if (foundAccount != null)
                {
                    cleanup.MarkForDeletion(foundAccount);
                }

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
                response.Headers.Location.ToString().Should().StartWith("/"); // default NextUri
                response.Headers.GetValues("Set-Cookie").Should().Contain(x => x.StartsWith("access_token="));
                response.Headers.GetValues("Set-Cookie").Should().Contain(x => x.StartsWith("refresh_token="));
            }
        }

        [Fact]
        public async Task RedirectToCustomNextUriIfAutologinIsEnabled()
        {
            // Arrange
            var config = new StormpathConfiguration()
            {
                Web = new WebConfiguration()
                {
                    Register = new WebRegisterRouteConfiguration()
                    {
                        AutoLogin = true,
                        NextUri = "/foobar"
                    }
                }
            };
            var fixture = new OwinTestFixture()
            {
                Options = new StormpathOwinOptions()
                {
                    Configuration = config
                }
            };
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var csrfToken = await CsrfToken.GetTokenForRoute(server, "/register");
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";

                var payload = new Dictionary<string, string>()
                {
                    ["email"] = email,
                    ["password"] = "Changeme123!!",
                    ["givenName"] = nameof(RedirectToLogin),
                    ["surname"] = nameof(RegisterRouteShould),
                    ["st"] = csrfToken,
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "/register")
                {
                    Content = new FormUrlEncodedContent(payload)
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                // Act
                var response = await server.SendAsync(request);

                var foundAccount = await application.GetAccounts().Where(a => a.Email == email).SingleOrDefaultAsync();
                if (foundAccount != null)
                {
                    cleanup.MarkForDeletion(foundAccount);
                }

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Redirect);
                response.Headers.Location.ToString().Should().StartWith("/foobar"); // default NextUri
                response.Headers.GetValues("Set-Cookie").Should().Contain(x => x.StartsWith("access_token="));
                response.Headers.GetValues("Set-Cookie").Should().Contain(x => x.StartsWith("refresh_token="));
            }
        }
    }
}
