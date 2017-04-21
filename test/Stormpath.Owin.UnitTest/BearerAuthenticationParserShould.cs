using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Stormpath.Owin.Middleware;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class BearerAuthenticationParserShould
    {
        [Fact]
        public void ReportNullHeaderInvalid()
        {
            var parser = new BearerAuthenticationParser(null, NullLogger.Instance);
            parser.IsValid.Should().BeFalse();
            parser.Token.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ReportEmptyHeaderInvalid()
        {
            var parser = new BearerAuthenticationParser(string.Empty, NullLogger.Instance);
            parser.IsValid.Should().BeFalse();
            parser.Token.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ReportNonBasicHeaderInvalid()
        {
            var parser = new BearerAuthenticationParser("Basic foobar", NullLogger.Instance);
            parser.IsValid.Should().BeFalse();
            parser.Token.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ReportEmptyPayloadInvalid()
        {
            var parser = new BearerAuthenticationParser("Bearer ", NullLogger.Instance);
            parser.IsValid.Should().BeFalse();
            parser.Token.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ParseValidPayload()
        {
            var parser = new BearerAuthenticationParser("Bearer foobar", NullLogger.Instance);
            parser.IsValid.Should().BeTrue();
            parser.Token.Should().Be("foobar");
        }
    }
}
