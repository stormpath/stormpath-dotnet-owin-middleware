using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public void ParsesEmptyHeaders(string[] input)
        {
            var parsed = new CookieParser(input, logger: null);

            parsed.Count.Should().Be(0);
        }

        //public void ParsesCookie(string )
    }
}
