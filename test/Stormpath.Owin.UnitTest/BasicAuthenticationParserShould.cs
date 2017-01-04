using FluentAssertions;
using Stormpath.Owin.Middleware;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class BasicAuthenticationParserShould
    {
        [Fact]
        public void ReportNullHeaderInvalid()
        {
            var parser = new BasicAuthenticationParser(null, null);
            parser.IsValid.Should().BeFalse();
            parser.Username.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ReportEmptyHeaderInvalid()
        {
            var parser = new BasicAuthenticationParser(string.Empty, null);
            parser.IsValid.Should().BeFalse();
            parser.Username.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ReportNonBasicHeaderInvalid()
        {
            var parser = new BasicAuthenticationParser("Bearer foobar", null);
            parser.IsValid.Should().BeFalse();
            parser.Username.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ReportEmptyPayloadInvalid()
        {
            var parser = new BasicAuthenticationParser("Basic ", null);
            parser.IsValid.Should().BeFalse();
            parser.Username.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ReportBadPayloadInvalid()
        {
            var parser = new BasicAuthenticationParser("Basic foobar", null);
            parser.IsValid.Should().BeFalse();
            parser.Username.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ParseValidPayload()
        {
            var parser = new BasicAuthenticationParser("Basic Zm9vOmJhcg==", null);
            parser.IsValid.Should().BeTrue();
            parser.Username.Should().Be("foo");
            parser.Password.Should().Be("bar");
        }
    }
}
