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
            yield return new object[] { new string[] { string.Empty } };
            yield return new object[] { new string[] { " " } };
            yield return new object[] { new string[] { " ," } };
            yield return new object[] { new string[] { " ;" } };
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
            yield return new object[] { new string[] { "access_token=eyJra.eyJqd.hGdm; foo=bar; baz=qux" } };
            yield return new object[] { new string[] { "access_token=eyJra.eyJqd.hGdm, foo=bar; baz=qux" } };
            yield return new object[] { new string[] { "  access_token=eyJra.eyJqd.hGdm;     foo=bar;; " } };
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
