using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Stormpath.SDK;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    public class RegisterRouteShould
    {
        private static async Task<string> GetCsrfToken(HttpClient server, string path)
        {
            var pageRequest = new HttpRequestMessage(HttpMethod.Get, path);
            pageRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

            var pageResponse = await server.SendAsync(pageRequest);
            pageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            pageResponse.Content.Headers.ContentType.MediaType.Should().Be("text/html");

            var pageContent = await pageResponse.Content.ReadAsStringAsync();
            var tokenFinder = new CsrfTokenFinder(pageContent);

            return tokenFinder.Token;
        }

        [Fact]
        public async Task RedirectToLogin()
        {
            // Arrange
            var fixture = new OwinTestFixture();
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";

                var csrfToken = await GetCsrfToken(server, "/register");

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
            throw new NotImplementedException();
        }

        [Fact]
        public async Task RedirectToCustomNextUriIfAutologinIsEnabled()
        {
            throw new NotImplementedException();
        }
    }
}
