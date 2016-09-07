using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Stormpath.Owin.Middleware;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Resource;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    [Collection(nameof(IntegrationTestCollection))]
    public class RequireCustomDataFilterShould
    {
        private readonly IntegrationTestFixture _fixture;

        public RequireCustomDataFilterShould(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        private IAccount NewTestAccount(IClient client)
            => client.Instantiate<IAccount>()
                .SetGivenName("Test")
                .SetSurname("User")
                .SetEmail($"{Guid.NewGuid()}@foo.bar")
                .SetPassword("Changeme123!!");

        [Fact]
        public async Task ReturnFalseForMissingCustomDataAsync()
        {
            IAccount testAccount = null;

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                return new IResource[] {testAccount};
            }))
            {
                var filter = new RequireCustomDataFilter("foobar", true);
                (await filter.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeFalse();
            }
        }

        [Fact]
        public void ReturnFalseForMissingCustomData()
        {
            IAccount testAccount = null;

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                return new IResource[] {testAccount};
            }))
            {
                var filter = new RequireCustomDataFilter("foobar", true);
                filter.IsAuthorized(testAccount).Should().BeFalse();
            }
        }

        [Fact]
        public async Task ReturnFalseForNonMatchingCustomDataAsync()
        {
            IAccount testAccount = null;

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                testAccount.CustomData["foobar"] = false;
                await testAccount.SaveAsync();
                return new IResource[] { testAccount };
            }))
            {
                var filter = new RequireCustomDataFilter("foobar", true);
                (await filter.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeFalse();
            }
        }

        [Fact]
        public void ReturnFalseForNonMatchingCustomData()
        {
            IAccount testAccount = null;

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                testAccount.CustomData["foobar"] = false;
                await testAccount.SaveAsync();
                return new IResource[] { testAccount };
            }))
            {
                var filter = new RequireCustomDataFilter("foobar", true);
                filter.IsAuthorized(testAccount).Should().BeFalse();
            }
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
            IAccount testAccount = null;

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                testAccount.CustomData["foobar"] = data;
                await testAccount.SaveAsync();
                return new IResource[] { testAccount };
            }))
            {
                var filter = new RequireCustomDataFilter("foobar", data);
                (await filter.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeTrue();
            }
        }

        [Theory(Skip = "Sync tests don't work")]
        [MemberData(nameof(MatchingCustomDataCases))]
        public void ReturnTrueForMatchingCustomData(object data)
        {
            IAccount testAccount = null;

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                testAccount.CustomData["foobar"] = data;
                await testAccount.SaveAsync();
                return new IResource[] { testAccount };
            }))
            {
                var filter = new RequireCustomDataFilter("foobar", data);
                filter.IsAuthorized(testAccount).Should().BeTrue();
            }
        }
    }
}
