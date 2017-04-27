using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Stormpath.Owin.Middleware;
using Xunit;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.UnitTest
{
    public class RequireGroupsFilterShould
    {
        [Fact]
        public async Task ReturnFalseForNullAccountAsync()
        {
            IOktaClient mockOktaClient = null;
            var filter = new RequireGroupsFilter(mockOktaClient, new[] { "group1"});
            (await filter.IsAuthorizedAsync(null, CancellationToken.None)).Should().BeFalse();
        }

        [Fact]
        public void ReturnFalseForNullAccount()
        {
            IOktaClient mockOktaClient = null;
            var filter = new RequireGroupsFilter(mockOktaClient, new[] { "group1" });
            filter.IsAuthorized(null).Should().BeFalse();
        }
    }
}
