using System.Linq;
using FluentAssertions;
using Stormpath.Owin.Middleware.Internal;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class ScopeParserShould
    {
        [Fact]
        public void ParseNull()
        {
            var parser = new ScopeParser(null);

            parser.Any().Should().BeFalse();
        }

        [Fact]
        public void ParseEmptyString()
        {
            var parser = new ScopeParser(string.Empty);

            parser.Any().Should().BeFalse();
        }

        [Fact]
        public void ParseSingle()
        {
            var parser = new ScopeParser("openid");

            parser.Single().Should().Be("openid");
        }

        [Fact]
        public void ParseMany()
        {
            var parser = new ScopeParser("openid profile");

            parser.Should().ContainInOrder("openid", "profile");
        }

        [Fact]
        public void IgnoreExtraSpaces()
        {
            var parser = new ScopeParser(" openid    profile ");

            parser.Should().ContainInOrder("openid", "profile");
        }
    }
}
