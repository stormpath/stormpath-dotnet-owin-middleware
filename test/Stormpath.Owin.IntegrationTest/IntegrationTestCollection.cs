using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Stormpath.Owin.IntegrationTest
{
    [CollectionDefinition(nameof(IntegrationTestCollection))]
    public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
        // Intentionally left blank. This class only serves as an anchor for CollectionDefinitionAttribute.
    }
}
