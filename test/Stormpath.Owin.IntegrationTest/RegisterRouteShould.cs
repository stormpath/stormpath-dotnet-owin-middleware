using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
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

        [Fact]
        public async Task RejectUnknownCustomFieldOnFormPost()
        {
            // Arrange
            var fixture = new OwinTestFixture();
            var server = Helpers.CreateServer(fixture);
            var csrfToken = await CsrfToken.GetTokenForRoute(server, "/register");

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);

                // Act
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";
                // Act
                var payload = new Dictionary<string, string>()
                {
                    ["email"] = email,
                    ["password"] = "Changeme123!!",
                    ["givenName"] = "Galen",
                    ["surname"] = "Erso",
                    ["codename"] = "stardust",
                    ["st"] = csrfToken,
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "/register")
                {
                    Content = new FormUrlEncodedContent(payload)
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                await server.SendAsync(request);

                var account = await application.GetAccounts().Where(a => a.Email == email).SingleOrDefaultAsync();
                if (account != null)
                {
                    cleanup.MarkForDeletion(account);
                }

                // Assert
                account.Should().BeNull();
            }
        }

        [Fact]
        public async Task RejectUnknownRootCustomFieldOnJsonPost()
        {
            // Arrange
            var fixture = new OwinTestFixture();
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);

                // Act
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";
                // Act
                var payload = new
                {
                    email,
                    password = "Changeme123!!",
                    givenName = "Galen",
                    surname = "Erso",
                    codename = "stardust"
                };

                var response = await server.PostAsync(
                    "/register", 
                    new StringContent(JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"));

                var account = await application.GetAccounts().Where(a => a.Email == email).SingleOrDefaultAsync();
                if (account != null)
                {
                    cleanup.MarkForDeletion(account);
                }

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task RejectUnknownNestedCustomFieldOnJsonPost()
        {
            // Arrange
            var fixture = new OwinTestFixture();
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);

                // Act
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";
                // Act
                var payload = new
                {
                    email,
                    password = "Changeme123!!",
                    givenName = "Galen",
                    surname = "Erso",
                    customData = new
                    {
                        codename = "stardust"
                    }
                };

                var response = await server.PostAsync(
                    "/register",
                    new StringContent(JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"));

                var account = await application.GetAccounts().Where(a => a.Email == email).SingleOrDefaultAsync();
                if (account != null)
                {
                    cleanup.MarkForDeletion(account);
                }

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task AcceptCustomFieldsOnFormPost()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    Configuration = new StormpathConfiguration()
                    {
                        Web = new WebConfiguration()
                        {
                            Register = new WebRegisterRouteConfiguration()
                            {
                                Form = new WebRegisterRouteFormConfiguration()
                                {
                                    Fields = new Dictionary<string, WebFieldConfiguration>()
                                    {
                                        ["codename"] = new WebFieldConfiguration()
                                        {
                                            Required = true,
                                            Enabled = true,
                                            Label = "custom",
                                            Placeholder = "custom",
                                            Type = "text",
                                            Visible = true
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
            };
            var server = Helpers.CreateServer(fixture);
            var csrfToken = await CsrfToken.GetTokenForRoute(server, "/register");

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);

                // Act
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";
                // Act
                var payload = new Dictionary<string, string>()
                {
                    ["email"] = email,
                    ["password"] = "Changeme123!!",
                    ["givenName"] = "Galen",
                    ["surname"] = "Erso",
                    ["codename"] = "stardust",
                    ["st"] = csrfToken,
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "/register")
                {
                    Content = new FormUrlEncodedContent(payload)
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

                await server.SendAsync(request);

                var account = await application.GetAccounts().Where(a => a.Email == email).SingleAsync();
                cleanup.MarkForDeletion(account);

                var customData = await account.GetCustomDataAsync();

                // Assert
                customData["codename"].ToString().Should().Be("stardust");
            }

        }

        [Fact]
        public async Task AcceptRootCustomFieldsOnJsonPost()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    Configuration = new StormpathConfiguration()
                    {
                        Web = new WebConfiguration()
                        {
                            Register = new WebRegisterRouteConfiguration()
                            {
                                Form = new WebRegisterRouteFormConfiguration()
                                {
                                    Fields = new Dictionary<string, WebFieldConfiguration>()
                                    {
                                        ["codename"] = new WebFieldConfiguration()
                                        {
                                            Required = true,
                                            Enabled = true,
                                            Label = "custom",
                                            Placeholder = "custom",
                                            Type = "text",
                                            Visible = true
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
            };
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);

                // Act
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";
                // Act
                var payload = new
                {
                    email,
                    password = "Changeme123!!",
                    givenName = "Galen",
                    surname = "Erso",
                    codename = "stardust"
                };

                var response = await server.PostAsync("/register", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                var account = await application.GetAccounts().Where(a => a.Email == email).SingleAsync();
                cleanup.MarkForDeletion(account);

                var customData = await account.GetCustomDataAsync();

                // Assert
                customData["codename"].ToString().Should().Be("stardust");
            }
        }

        /// <summary>
        /// Regression test for https://github.com/stormpath/stormpath-dotnet-owin-middleware/issues/70
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AcceptNestedCustomFieldsOnJsonPost()
        {
            // Arrange
            var fixture = new OwinTestFixture
            {
                Options = new StormpathOwinOptions
                {
                    Configuration = new StormpathConfiguration()
                    {
                        Web = new WebConfiguration()
                        {
                            Register = new WebRegisterRouteConfiguration()
                            {
                                Form = new WebRegisterRouteFormConfiguration()
                                {
                                    Fields = new Dictionary<string, WebFieldConfiguration>()
                                    {
                                        ["codename"] = new WebFieldConfiguration()
                                        {
                                            Required = true,
                                            Enabled = true,
                                            Label = "custom",
                                            Placeholder = "custom",
                                            Type = "text",
                                            Visible = true
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
            };
            var server = Helpers.CreateServer(fixture);

            using (var cleanup = new AutoCleanup(fixture.Client))
            {
                var application = await fixture.Client.GetApplicationAsync(fixture.ApplicationHref);

                // Act
                var email = $"its-{fixture.TestKey}@testmail.stormpath.com";
                // Act
                var payload = new
                {
                    email,
                    password = "Changeme123!!",
                    givenName = "Galen",
                    surname = "Erso",
                    customData = new
                    {
                        codename = "stardust"
                    }
                };

                var response = await server.PostAsync("/register", new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                var account = await application.GetAccounts().Where(a => a.Email == email).SingleAsync();
                cleanup.MarkForDeletion(account);

                var customData = await account.GetCustomDataAsync();

                // Assert
                customData["codename"].ToString().Should().Be("stardust");
            }
        }
    }
}
