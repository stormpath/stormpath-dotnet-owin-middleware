using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    [CollectionDefinition(nameof(IntegrationTestCollection))]
    public class IntegrationTestCollection : ICollectionFixture<StandaloneTestFixture>
    {
        // Intentionally left blank. This class only serves as an anchor for CollectionDefinitionAttribute.
    }
}
