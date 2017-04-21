using System.Threading;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Stormpath.Owin.Middleware;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class BasicAuthenticationParserShould
    {
        [Fact]
        public void ReportNullHeaderInvalid()
        {
            var parser = new BasicAuthenticationParser(null, NullLogger.Instance);
            parser.IsValid.Should().BeFalse();
            parser.Username.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ReportEmptyHeaderInvalid()
        {
            var parser = new BasicAuthenticationParser(string.Empty, NullLogger.Instance);
            parser.IsValid.Should().BeFalse();
            parser.Username.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ReportNonBasicHeaderInvalid()
        {
            var parser = new BasicAuthenticationParser("Bearer foobar", NullLogger.Instance);
            parser.IsValid.Should().BeFalse();
            parser.Username.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ReportEmptyPayloadInvalid()
        {
            var parser = new BasicAuthenticationParser("Basic ", NullLogger.Instance);
            parser.IsValid.Should().BeFalse();
            parser.Username.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ReportBadPayloadInvalid()
        {
            var parser = new BasicAuthenticationParser("Basic foobar", NullLogger.Instance);
            parser.IsValid.Should().BeFalse();
            parser.Username.Should().BeNullOrEmpty();
        }

        [Theory]
        [InlineData("Basic Zm9vOmJhcg==", "foo", "bar")]
        [InlineData("Basic NVhHR1I4SVFJVkhKSlBOS0VaUjkzNktYUjo1WFU3Yy9YM0lKRkRtUit6U1pINzdqMVdRdHlKQWtGL0I3N3AwVUN3MEZr", "5XGGR8IQIVHJJPNKEZR936KXR", "5XU7c/X3IJFDmR+zSZH77j1WQtyJAkF/B77p0UCw0Fk")]
        public void ParseValidPayload(string header, string username, string password)
        {
            var parser = new BasicAuthenticationParser(header, null);
            parser.IsValid.Should().BeTrue();
            parser.Username.Should().Be(username);
            parser.Password.Should().Be(password);
        }
    }
}
