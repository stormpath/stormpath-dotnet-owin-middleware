using System.Collections.Generic;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;

namespace Stormpath.Owin.UnitTest
{
    public static class ConfigurationHelper
    {
        public static IntegrationConfiguration CreateFakeConfiguration(StormpathConfiguration config)
        {
            var compiledConfig = Configuration.ConfigurationLoader.Initialize().Load(config);

            var integrationConfig = new IntegrationConfiguration(
                compiledConfig, 
                new TenantConfiguration("foo", false, false),
                new KeyValuePair<string, ProviderConfiguration>[0]);

            return integrationConfig;
        }
    }
}
