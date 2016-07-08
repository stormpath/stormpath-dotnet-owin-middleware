using System.Collections.Generic;
using FluentAssertions;
using Stormpath.Owin.Middleware.Internal;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class CookieParserTests
    {
        public static IEnumerable<object[]> EmptyTestCases()
        {
            yield return new object[] { new string[] { null } };
            yield return new object[] { new[] { string.Empty } };
            yield return new object[] { new[] { " " } };
            yield return new object[] { new[] { " ," } };
            yield return new object[] { new[] { " ;" } };
        }

        [Theory]
        [MemberData(nameof(EmptyTestCases))]
        public void ParsesEmptyHeaders(string[] headers)
        {
            var parsed = new CookieParser(headers, logger: null);

            parsed.Count.Should().Be(0);
        }

        public static IEnumerable<object[]> TestCases()
        {
            yield return new object[] { new[] { "access_token=eyJra.eyJqd.hGdm; foo=bar; baz=qux" } };
            yield return new object[] { new[] { "access_token=eyJra.eyJqd.hGdm, foo=bar; baz=qux" } };
            yield return new object[] { new[] { "  access_token=eyJra.eyJqd.hGdm;     foo=bar;; " } };
            yield return new object[] { new[] { "tricky=base_domain=foo.bar; access_token=eyJra.eyJqd.hGdm;" } };
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void ParsesCookie(string[] headers)
        {
            var parsed = new CookieParser(headers, logger: null);

            parsed.Get("access_token").Should().Be("eyJra.eyJqd.hGdm");
        }
    }
}
