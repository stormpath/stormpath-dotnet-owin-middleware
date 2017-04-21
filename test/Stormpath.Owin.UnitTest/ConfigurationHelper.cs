using System.Collections.Generic;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;

namespace Stormpath.Owin.UnitTest
{
    public static class ConfigurationHelper
    {
        public static IntegrationConfiguration CreateFakeConfiguration(StormpathConfiguration config)
        {
            config.ApiToken = config.ApiToken ?? "fooApiToken";
            config.Org = config.Org ?? "https://dev-12345.oktapreview.foo";

            config.Application = config.Application ?? new OktaApplicationConfiguration();
            config.Application.Id = config.Application.Id ?? "abcd1234xyz";

            var compiledConfig = Configuration.ConfigurationLoader.Initialize().Load(config);

            var integrationConfig = new IntegrationConfiguration(
                compiledConfig, 
                new OktaEnvironmentConfiguration("fooAuthServerId", "fooClientId", "fooClientSecret123456"),
                new KeyValuePair<string, ProviderConfiguration>[0]);

            return integrationConfig;
        }
    }
}
