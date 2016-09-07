using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Stormpath.Owin.Middleware;
using Stormpath.SDK.Account;
using Stormpath.SDK.CustomData;
using Stormpath.SDK.Sync;
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

        [Fact]
        public async Task ReturnFalseForMissingCustomDataAsync()
        {
            var testAccount = Substitute.For<IAccount>();
            testAccount.GetCustomDataAsync().Returns(Task.FromResult(Substitute.For<ICustomData>()));

            var filter = new RequireCustomDataFilter("foobar", true);
            (await filter.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeFalse();
        }

        [Fact(Skip = "Sync tests don't work")]
        public void ReturnFalseForMissingCustomData()
        {
            var testAccount = Substitute.For<IAccount>();
            testAccount.GetCustomData().Returns(Substitute.For<ICustomData>());

            var filter = new RequireCustomDataFilter("foobar", true);
            filter.IsAuthorized(testAccount).Should().BeFalse();
        }

        [Fact]
        public async Task ReturnFalseForNonMatchingCustomDataAsync()
        {
            var testCustomData = Substitute.For<ICustomData>();
            testCustomData["foobar"].Returns(false);

            var testAccount = Substitute.For<IAccount>();
            testAccount.GetCustomDataAsync().Returns(Task.FromResult(testCustomData));

            var filter = new RequireCustomDataFilter("foobar", true);
            (await filter.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeFalse();
        }

        [Fact(Skip = "Sync tests don't work")]
        public void ReturnFalseForNonMatchingCustomData()
        {
            var testCustomData = Substitute.For<ICustomData>();
            testCustomData["foobar"].Returns(false);

            var testAccount = Substitute.For<IAccount>();
            testAccount.GetCustomDataAsync().Returns(Task.FromResult(testCustomData));

            var filter = new RequireCustomDataFilter("foobar", true);
            filter.IsAuthorized(testAccount).Should().BeFalse();
        }

        public static IEnumerable<object[]> MatchingCustomDataCases()
        {
            yield return new object[] { true };
            yield return new object[] { "hello world!" };
            yield return new object[] { 1234 };
        }

        [Theory]
        [MemberData(nameof(MatchingCustomDataCases))]
        public async Task ReturnTrueForMatchingCustomDataAsync(object data)
        {
            var testCustomData = Substitute.For<ICustomData>();
            testCustomData["foobar"].Returns(data);

            var testAccount = Substitute.For<IAccount>();
            testAccount.GetCustomDataAsync().Returns(Task.FromResult(testCustomData));

            var filter = new RequireCustomDataFilter("foobar", data);
            (await filter.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeTrue();
        }

        [Theory(Skip = "Sync tests don't work")]
        [MemberData(nameof(MatchingCustomDataCases))]
        public void ReturnTrueForMatchingCustomData(object data)
        {
            var testCustomData = Substitute.For<ICustomData>();
            testCustomData["foobar"].Returns(data);

            var testAccount = Substitute.For<IAccount>();
            testAccount.GetCustomDataAsync().Returns(Task.FromResult(testCustomData));

            var filter = new RequireCustomDataFilter("foobar", data);
            filter.IsAuthorized(testAccount).Should().BeTrue();
        }
    }
}
