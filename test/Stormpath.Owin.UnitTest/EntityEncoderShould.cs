using FluentAssertions;
using Stormpath.Owin.Abstractions;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class EntityEncoderShould
    {
        [Fact]
        public void EncodeAmpersand() => EntityEncoder.Encode("stuff&").Should().Be("stuff&amp;");

        [Fact]
        public void EncodeAngles() => EntityEncoder.Encode("<script>").Should().Be("&lt;script&gt;");

        [Fact]
        public void EncodeQuotes() => EntityEncoder.Encode("that's cool\"").Should().Be("that&#x27;s cool&quot;");

        [Fact]
        public void EncodeSlash() => EntityEncoder.Encode("/closing").Should().Be("&#x2F;closing");
    }
}