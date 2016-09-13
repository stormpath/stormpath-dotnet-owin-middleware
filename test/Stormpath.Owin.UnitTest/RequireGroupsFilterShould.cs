using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Stormpath.Owin.Middleware;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class RequireGroupsFilterShould
    {
        [Fact]
        public async Task ReturnFalseForNullAccountAsync()
        {
            var filter = new RequireGroupsFilter(new[] { "group1"});
            (await filter.IsAuthorizedAsync(null, CancellationToken.None)).Should().BeFalse();
        }

        [Fact]
        public void ReturnFalseForNullAccount()
        {
            var filter = new RequireGroupsFilter(new[] { "group1" });
            filter.IsAuthorized(null).Should().BeFalse();
        }
    }
}
