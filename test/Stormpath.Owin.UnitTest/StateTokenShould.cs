using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Stormpath.Owin.Middleware;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class StateTokenShould
    {
        private const string TestSecret = "superSecretKey_123!!";

        [Fact]
        public void RoundtripTokenWithPath()
        {
            var builder = new StateTokenBuilder(TestSecret);
            builder.Path = "/foo/bar/9";

            var result = builder.ToString();
            var parser = new StateTokenParser(TestSecret, result, NullLogger.Instance);

            parser.Valid.Should().BeTrue();
            parser.Path.Should().Be("/foo/bar/9");
            parser.State.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void RoundtripTokenWithPathAndState()
        {
            var builder = new StateTokenBuilder(TestSecret);
            builder.Path = "/foo/bar/9";
            builder.State = "asdf1234!?";

            var result = builder.ToString();
            var parser = new StateTokenParser(TestSecret, result, NullLogger.Instance);

            parser.Valid.Should().BeTrue();
            parser.Path.Should().Be("/foo/bar/9");
            parser.State.Should().Be("asdf1234!?");
        }

        [Fact]
        public void FailValidationForIncorrectSecret()
        {
            var builder = new StateTokenBuilder("notTheCorrectSecret987");
            builder.Path = "/hello";

            var result = builder.ToString();
            var parser = new StateTokenParser(TestSecret, result, NullLogger.Instance);

            parser.Valid.Should().BeFalse();
            parser.Path.Should().BeNull();
        }

        [Fact]
        public void FailValidationForEmptyToken()
        {
            var parser = new StateTokenParser(TestSecret, string.Empty, NullLogger.Instance);

            parser.Valid.Should().BeFalse();
            parser.Path.Should().BeNull();
        }

        [Fact]
        public void FailValidationForExpiredToken()
        {
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE0OTA5MjE4NTUsInBhdGgiOiIvZm9vL2Jhci85Iiwic3RhdGUiOiI2OGE0OTRiOC0yYmE0LTRiMzctOGU0Zi1mZDRhYjM5YmYxYjEiLCJqdGkiOiIzMWQ1OTYxNC01OTg2LTQ3MjItOWM2Ny0xMzZkMzg2ZTRkMjMiLCJpYXQiOjE0OTA5MTgyNTV9.zgxYy-otE008W5b8AcFaOkCOE9QAZ5Hlxl94kObK_8Q";
            var parser = new StateTokenParser(TestSecret, token, NullLogger.Instance);

            parser.Valid.Should().BeFalse();
            parser.Path.Should().BeNull();
        }
    }
}
