using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Middleware;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    public class Oauth2RouteShould
    {
        [Fact]
        public async Task RejectRequestsWhenRouteDisabled()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    Configuration = new StormpathConfiguration
                    {
                        Web = new WebConfiguration
                        {
                            Oauth2 = new WebOauth2RouteConfiguration
                            {
                                Enabled = false
                            }
                        }
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            var payload = new Dictionary<string, string>()
            {
                ["grant_type"] = "notARealRequest",
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/oauth/token")
            {
                Content = new FormUrlEncodedContent(payload)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await server.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RejectRequestsWhenPasswordGrantDisabled()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    Configuration = new StormpathConfiguration
                    {
                        Web = new WebConfiguration
                        {
                            Oauth2 = new WebOauth2RouteConfiguration
                            {
                                Password = new WebOauth2PasswordGrantConfiguration
                                {
                                    Enabled = false
                                }
                            }
                        }
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            var payload = new Dictionary<string, string>()
            {
                ["grant_type"] = "password",
                ["username"] = "foobar",
                ["password"] = "baz123!"
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/oauth/token")
            {
                Content = new FormUrlEncodedContent(payload)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await server.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
            body["error"].Should().Be("unsupported_grant_type");
        }

        [Fact]
        public async Task RejectRequestsWhenClientCredentialsGrantDisabled()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    Configuration = new StormpathConfiguration
                    {
                        Web = new WebConfiguration
                        {
                            Oauth2 = new WebOauth2RouteConfiguration
                            {
                                Client_Credentials = new WebOauth2ClientCredentialsGrantConfiguration
                                {
                                    Enabled = false
                                }
                            }
                        }
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            var payload = new Dictionary<string, string>()
            {
                ["grant_type"] = "client_credentials"
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/oauth/token")
            {
                Content = new FormUrlEncodedContent(payload)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await server.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
            body["error"].Should().Be("unsupported_grant_type");
        }
    }
}