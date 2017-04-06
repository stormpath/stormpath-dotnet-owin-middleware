using FluentAssertions;
using Newtonsoft.Json;
using Stormpath.Owin.Middleware;
using System.Collections.Generic;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class StringOrListConverterShould
    {
        private class PocoUnderTest
        {
            [JsonConverter(typeof(StringOrListConverter))]
            public IList<string> Foo { get; set; }
        }

        [Fact]
        public void DeserializeStringAsSingleItemList()
        {
            var json = @"{ ""foo"": ""bar"" }";

            var result = JsonConvert.DeserializeObject<PocoUnderTest>(json);
            result.Foo.Count.Should().Be(1);
            result.Foo.Should().BeEquivalentTo("bar");
        }

        [Fact]
        public void DeserializeArrayAsIList()
        {
            var json = @"{ ""foo"": [ ""bar"", ""baz"" ] }";

            var result = JsonConvert.DeserializeObject<PocoUnderTest>(json);
            result.Foo.Count.Should().Be(2);
            result.Foo.Should().BeEquivalentTo("bar", "baz");
        }
    }
}
