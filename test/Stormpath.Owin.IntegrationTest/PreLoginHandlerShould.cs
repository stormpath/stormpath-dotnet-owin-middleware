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
using Stormpath.SDK;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Organization;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    public class PreLoginHandlerShould
    {
        private static string EnsurePadding(string base64)
        {
            // Padding is truncated in JWTs, so we might need to fix that
            while (base64.Length % 4 != 0)
            {
                base64 += "=";
            }

            return base64;
        }

        [Fact]
        public async Task AlterLogin()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PreLoginHandler = (ctx, ct) =>
                    {
                        ctx.Login = ctx.Login + ".com";
                        return Task.FromResult(0);
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                var account = await application.CreateAccountAsync(
                    nameof(AlterLogin),
                    nameof(PreLoginHandlerShould),
                    $"its-{fixture.TestKey}@testmail.stormpath.com",
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var payload = new
                {
                    login = $"its-{fixture.TestKey}@testmail.stormpath", // missing ".com"
                    password = "Changeme123!!"
                };

                // Act
                var response = await server.PostAsync("/login", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue();
            }
        }

        [Fact]
        public async Task SpecifyOrganization()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PreLoginHandler = async (ctx, ct) =>
                    {
                        ctx.AccountStore = await ctx.Client.GetOrganizations()
                            .Where(org => org.NameKey == "TestOrg")
                            .SingleAsync(ct);
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                // Create an organization
                var org = fixture.Client.Instantiate<IOrganization>()
                    .SetName($"Test Organization {fixture.TestKey}")
                    .SetNameKey("TestOrg");
                await fixture.Client.CreateOrganizationAsync(org, opt => opt.CreateDirectory = true);
                cleanup.MarkForDeletion(org);
                
                var createdDirectory = await fixture.Client.GetDirectories().Where(dir => dir.Name.StartsWith($"Test Organization {fixture.TestKey}")).SingleAsync();
                //cleanup.MarkForDeletion(directory); // TODO
                
                // Create an account in the organization
                await org.CreateAccountAsync(
                    nameof(SpecifyOrganization),
                    nameof(PreLoginHandlerShould),
                    $"its-{fixture.TestKey}@testmail.stormpath.com",
                    "Changeme123!!");
                // Account will be deleted along with directory

                // Associate the org with our application
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                await application.AddAccountStoreAsync(org);

                var payload = new Dictionary<string, string>()
                {
                    ["grant_type"] = "password",
                    ["username"] = $"its-{fixture.TestKey}@testmail.stormpath.com",
                    ["password"] = "Changeme123!!"
                };

                // Act
                var response = await server.PostAsync("/oauth/token", new FormUrlEncodedContent(payload));

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue();
                var deserializedResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
                var accessToken = deserializedResponse["access_token"];

                var body = accessToken.Split('.')[1];
                body = EnsurePadding(body);

                var decodedJwt = Encoding.UTF8.GetString(Convert.FromBase64String(body));
                var deserializedClaims = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedJwt);
                deserializedClaims.Should().ContainKey("org");
                deserializedClaims["org"].Should().Be(org.Href);
            }
        }

        [Fact]
        public async Task SpecifyOrganizationByNameKey()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    PreLoginHandler = (ctx, ct) =>
                    {
                        ctx.OrganizationNameKey = "TestOrg";
                        return Task.CompletedTask;
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                // Create an organization
                var org = fixture.Client.Instantiate<IOrganization>()
                    .SetName($"Test Organization {fixture.TestKey}")
                    .SetNameKey("TestOrg");
                await fixture.Client.CreateOrganizationAsync(org, opt => opt.CreateDirectory = true);
                cleanup.MarkForDeletion(org);

                var createdDirectory = await fixture.Client.GetDirectories().Where(dir => dir.Name.StartsWith($"Test Organization {fixture.TestKey}")).SingleAsync();
                //cleanup.MarkForDeletion(directory); // TODO

                // Create an account in the organization
                await org.CreateAccountAsync(
                    nameof(SpecifyOrganizationByNameKey),
                    nameof(PreLoginHandlerShould),
                    $"its-{fixture.TestKey}@testmail.stormpath.com",
                    "Changeme123!!");
                // Account will be deleted along with directory

                // Associate the org with our application
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);
                await application.AddAccountStoreAsync(org);

                var payload = new Dictionary<string, string>()
                {
                    ["grant_type"] = "password",
                    ["username"] = $"its-{fixture.TestKey}@testmail.stormpath.com",
                    ["password"] = "Changeme123!!"
                };

                // Act
                var response = await server.PostAsync("/oauth/token", new FormUrlEncodedContent(payload));

                // Assert
                response.IsSuccessStatusCode.Should().BeTrue();
                var deserializedResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
                var accessToken = deserializedResponse["access_token"];

                var body = accessToken.Split('.')[1];
                body = EnsurePadding(body);

                var decodedJwt = Encoding.UTF8.GetString(Convert.FromBase64String(body));
                var deserializedClaims = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedJwt);
                deserializedClaims.Should().ContainKey("org");
                deserializedClaims["org"].Should().Be(org.Href);
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
                    PreLoginHandler = (ctx, ct) =>
                    {
                        ctx.Result = new PreLoginResult()
                        {
                            Success = false
                        };
                        return Task.FromResult(0);
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);
            var csrfToken = await CsrfToken.GetTokenForRoute(server, "/login");

            // Act
            var payload = new Dictionary<string, string>()
            {
                ["login"] = "jyn@testmail.stormpath.com",
                ["password"] = "Changeme123!!",
                ["st"] = csrfToken,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/login")
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
                    PreLoginHandler = (ctx, ct) =>
                    {
                        ctx.Result = new PreLoginResult()
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
                login = "jyn@testmail.stormpath.com",
                password = "Changeme123!!",
            };

            var response = await server.PostAsync("/login", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

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
                    PreLoginHandler = (ctx, ct) =>
                    {
                        ctx.Result = new PreLoginResult()
                        {
                            Success = false,
                            ErrorMessage = "Nice try, rebel scum!"
                        };
                        return Task.FromResult(0);
                    }
                }
            };
            var server = Helpers.CreateServer(fixture);
            var csrfToken = await CsrfToken.GetTokenForRoute(server, "/login");

            // Act
            var payload = new Dictionary<string, string>()
            {
                ["login"] = "jyn@testmail.stormpath.com",
                ["password"] = "Changeme123!!",
                ["st"] = csrfToken,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/login")
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
                    PreLoginHandler = (ctx, ct) =>
                    {
                        ctx.Result = new PreLoginResult()
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
                login = "jyn@testmail.stormpath.com",
                password = "Changeme123!!",
            };

            var response = await server.PostAsync("/login", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await response.Content.ReadAsStringAsync()).Should().Contain("\"message\": \"Nice try, rebel scum!\"");
        }
    }
}