using FluentAssertions;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.Owin.Abstractions;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class StateTokenShould
    {
        private ClientApiKeyConfiguration GetApiKey()
            => new ClientApiKeyConfiguration(id: "fake", secret: "superduperfake123!");

        [Fact]
        public void RoundtripTokenWithPath()
        {
            var builder = new StateTokenBuilder(GetApiKey());
            builder.Path = "/foo/bar/9";

            var result = builder.ToString();
            var parser = new StateTokenParser(GetApiKey(), result, null);

            parser.Valid.Should().BeTrue();
            parser.Path.Should().Be("/foo/bar/9");
            parser.State.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void RoundtripTokenWithPathAndState()
        {
            var builder = new StateTokenBuilder(GetApiKey());
            builder.Path = "/foo/bar/9";
            builder.State = "asdf1234!?";

            var result = builder.ToString();
            var parser = new StateTokenParser(GetApiKey(), result, null);

            parser.Valid.Should().BeTrue();
            parser.Path.Should().Be("/foo/bar/9");
            parser.State.Should().Be("asdf1234!?");
        }

        [Fact]
        public void FailValidationForIncorrectSecret()
        {
            var builder = new StateTokenBuilder(new ClientApiKeyConfiguration(id: "foo", secret: "notTheCorrectSecret987"));
            builder.Path = "/hello";

            var result = builder.ToString();
            var parser = new StateTokenParser(GetApiKey(), result, null);

            parser.Valid.Should().BeFalse();
            parser.Path.Should().BeNull();
        }
    }
}
