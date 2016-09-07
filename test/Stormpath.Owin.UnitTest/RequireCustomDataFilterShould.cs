using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Stormpath.Owin.Middleware;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class RequireCustomDataFilterShould
    {
        [Fact]
        public async Task ReturnFalseForNullAccountAsync()
        {
            var filter = new RequireCustomDataFilter("foobar", true);
            (await filter.IsAuthorizedAsync(null, CancellationToken.None)).Should().BeFalse();
        }

        [Fact]
        public void ReturnFalseForNullAccount()
        {
            var filter = new RequireCustomDataFilter("foobar", true);
            filter.IsAuthorized(null).Should().BeFalse();
        }
    }
}
