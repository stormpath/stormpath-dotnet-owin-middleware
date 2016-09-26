using FluentAssertions;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Middleware;
using Stormpath.SDK.Client;
using Stormpath.SDK.Http;
using Stormpath.SDK.Serialization;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class RedirectTokenShould
    {
        private ClientApiKeyConfiguration GetApiKey()
            => new ClientApiKeyConfiguration(id: "fake", secret: "superduperfake123!");

        private IClient CreateClient()
        {
            return Clients.Builder()
                .SetApiKeyId(GetApiKey().Id)
                .SetApiKeySecret(GetApiKey().Secret)
                .SetHttpClient(HttpClients.Create().SystemNetHttpClient())
                .SetSerializer(Serializers.Create().JsonNetSerializer())
                .Build();
        }

        [Fact]
        public void RoundtripTokenWithPath()
        {
            var client = CreateClient();
            var builder = new RedirectTokenBuilder(client, GetApiKey());
            builder.Path = "/foo/bar/9";

            var result = builder.ToString();
            var parser = new RedirectTokenParser(client, GetApiKey(), result, null);

            parser.Valid.Should().BeTrue();
            parser.Path.Should().Be("/foo/bar/9");
            parser.State.Should().BeNull();
        }

        [Fact]
        public void RoundtripTokenWithPathAndState()
        {
            var client = CreateClient();
            var builder = new RedirectTokenBuilder(client, GetApiKey());
            builder.Path = "/foo/bar/9";
            builder.State = "asdf1234!?";

            var result = builder.ToString();
            var parser = new RedirectTokenParser(client, GetApiKey(), result, null);

            parser.Valid.Should().BeTrue();
            parser.Path.Should().Be("/foo/bar/9");
            parser.State.Should().Be("asdf1234!?");
        }

        [Fact]
        public void FailValidationForIncorrectSecret()
        {
            var client = CreateClient();
            var builder = new RedirectTokenBuilder(client, new ClientApiKeyConfiguration(id: "foo", secret: "notTheCorrectSecret987"));
            builder.Path = "/hello";

            var result = builder.ToString();
            var parser = new RedirectTokenParser(client, GetApiKey(), result, null);

            parser.Valid.Should().BeFalse();
            parser.Path.Should().BeNull();
        }
    }
}
