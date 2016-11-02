using System;
using System.Collections.Generic;
using System.Net.Http;
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
                    $"its-{fixture.TestKey}@example.com",
                    "Changeme123!!");
                cleanup.MarkForDeletion(account);

                var payload = new
                {
                    login = $"its-{fixture.TestKey}@example", // missing ".com"
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

                // Padding is truncated in JWTs, so we might need to fix that
                body += new string('=', body.Length % 4);

                var decodedJwt = Encoding.UTF8.GetString(Convert.FromBase64String(body));
                var deserializedClaims = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedJwt);
                deserializedClaims.Should().ContainKey("org");
                deserializedClaims["org"].Should().Be(org.Href);
            }
        }
    }
}