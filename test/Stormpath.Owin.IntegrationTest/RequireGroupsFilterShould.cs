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
    public class RequireGroupsFilterShould
    {
        private readonly IntegrationTestFixture _fixture;

        public RequireGroupsFilterShould(IntegrationTestFixture fixture)
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
        public async Task ReturnFalseForMissingGroupAsync()
        {
            IAccount testAccount = null;

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                return new IResource[] {testAccount};
            }))
            {
                var filter = new RequireGroupsFilter(new[] {"testGroup"});
                (await filter.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeFalse();
            }
        }

        [Fact]
        public void ReturnFalseForMissingGroup()
        {
            IAccount testAccount = null;

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                return new IResource[] { testAccount };
            }))
            {
                var filter = new RequireGroupsFilter(new[] { "testGroup" });
                filter.IsAuthorized(testAccount).Should().BeFalse();
            }
        }

        [Fact]
        public async Task ReturnTrueForGroupByNameAsync()
        {
            IAccount testAccount = null;
            var group1Name = $"group1-{_fixture.TestInstanceKey}";
            var group2Name = $"group2-{_fixture.TestInstanceKey}";

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                var group1 = await _fixture.TestDirectory.CreateGroupAsync(group1Name, $"Stormpath.Owin IT {_fixture.TestInstanceKey}");
                var group2 = await _fixture.TestDirectory.CreateGroupAsync(group2Name, $"Stormpath.Owin IT {_fixture.TestInstanceKey}");
                await testAccount.AddGroupAsync(group1);
                await testAccount.AddGroupAsync(group2);
                return new IResource[] { testAccount, group1, group2 };
            }))
            {
                var filter1 = new RequireGroupsFilter(new[] { group1Name });
                (await filter1.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeTrue();

                var filter2 = new RequireGroupsFilter(new[] { group2Name });
                (await filter2.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeTrue();

                var filterBoth = new RequireGroupsFilter(new[] { group1Name, group2Name });
                (await filterBoth.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeTrue();
            }
        }

        [Fact]
        public void ReturnTrueForGroupByName()
        {
            IAccount testAccount = null;
            var group1Name = $"group1-{_fixture.TestInstanceKey}";
            var group2Name = $"group2-{_fixture.TestInstanceKey}";

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                var group1 = await _fixture.TestDirectory.CreateGroupAsync(group1Name, $"Stormpath.Owin IT {_fixture.TestInstanceKey}");
                var group2 = await _fixture.TestDirectory.CreateGroupAsync(group2Name, $"Stormpath.Owin IT {_fixture.TestInstanceKey}");
                await testAccount.AddGroupAsync(group1);
                await testAccount.AddGroupAsync(group2);
                return new IResource[] { testAccount, group1, group2 };
            }))
            {
                var filter1 = new RequireGroupsFilter(new[] { group1Name });
                filter1.IsAuthorized(testAccount).Should().BeTrue();

                var filter2 = new RequireGroupsFilter(new[] { group2Name });
                filter2.IsAuthorized(testAccount).Should().BeTrue();

                var filterBoth = new RequireGroupsFilter(new[] { group1Name, group2Name });
                filterBoth.IsAuthorized(testAccount).Should().BeTrue();
            }
        }

        [Fact]
        public async Task ReturnTrueForGroupByHrefAsync()
        {
            IAccount testAccount = null;
            string group1Href = null, group2Href = null;

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                var group1 = await _fixture.TestDirectory.CreateGroupAsync($"group1-{Guid.NewGuid()}", $"Stormpath.Owin IT {_fixture.TestInstanceKey}");
                var group2 = await _fixture.TestDirectory.CreateGroupAsync($"group1-{Guid.NewGuid()}", $"Stormpath.Owin IT {_fixture.TestInstanceKey}");
                await testAccount.AddGroupAsync(group1);
                await testAccount.AddGroupAsync(group2);
                group1Href = group1.Href;
                group2Href = group2.Href;
                return new IResource[] { testAccount, group1, group2 };
            }))
            {
                var filter1 = new RequireGroupsFilter(new[] { group1Href });
                (await filter1.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeTrue();

                var filter2 = new RequireGroupsFilter(new[] { group2Href });
                (await filter2.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeTrue();

                var filterBoth = new RequireGroupsFilter(new[] { group1Href, group2Href });
                (await filterBoth.IsAuthorizedAsync(testAccount, CancellationToken.None)).Should().BeTrue();
            }
        }

        [Fact]
        public void ReturnTrueForGroupByHref()
        {
            IAccount testAccount = null;
            string group1Href = null, group2Href = null;

            using (new TestEnvironment(_fixture.Client, async c =>
            {
                testAccount = await _fixture.TestDirectory.CreateAccountAsync(NewTestAccount(c));
                var group1 = await _fixture.TestDirectory.CreateGroupAsync($"group1-{Guid.NewGuid()}", $"Stormpath.Owin IT {_fixture.TestInstanceKey}");
                var group2 = await _fixture.TestDirectory.CreateGroupAsync($"group1-{Guid.NewGuid()}", $"Stormpath.Owin IT {_fixture.TestInstanceKey}");
                await testAccount.AddGroupAsync(group1);
                await testAccount.AddGroupAsync(group2);
                group1Href = group1.Href;
                group2Href = group2.Href;
                return new IResource[] { testAccount, group1, group2 };
            }))
            {
                var filter1 = new RequireGroupsFilter(new[] { group1Href });
                filter1.IsAuthorized(testAccount).Should().BeTrue();

                var filter2 = new RequireGroupsFilter(new[] { group2Href });
                filter2.IsAuthorized(testAccount).Should().BeTrue();

                var filterBoth = new RequireGroupsFilter(new[] { group1Href, group2Href });
                filterBoth.IsAuthorized(testAccount).Should().BeTrue();
            }
        }
    }
}
