using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    public class LogoutRouteShould
    {
        [Fact]
        public async Task AcceptRequestWithValidContentType()
        {
            // Arrange
            var fixture = new OwinTestFixture();
            var server = Helpers.CreateServer(fixture);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/logout")
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await server.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AcceptRequestWithNoContentType()
        {
            // Arrange
            var fixture = new OwinTestFixture();
            var server = Helpers.CreateServer(fixture);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/logout");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await server.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
