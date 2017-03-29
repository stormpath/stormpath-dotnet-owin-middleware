using System.Collections.Generic;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Abstractions.Configuration;

namespace Stormpath.Owin.UnitTest
{
    public static class ConfigurationHelper
    {
        public static IntegrationConfiguration CreateFakeConfiguration(StormpathConfiguration config)
        {
            config.Okta = config.Okta ?? new OktaConfiguration();
            config.Okta.ApiToken = config.Okta.ApiToken ?? "fooApiToken";
            config.Okta.Org = config.Okta.Org ?? "https://dev-12345.oktapreview.foo";

            config.Okta.Application = config.Okta.Application ?? new OktaApplicationConfiguration();
            config.Okta.Application.Id = config.Okta.Application.Id ?? "abcd1234xyz";

            var compiledConfig = Configuration.ConfigurationLoader.Initialize().Load(config);

            var integrationConfig = new IntegrationConfiguration(
                compiledConfig, 
                new OktaEnvironmentConfiguration("fooClientId", "fooClientSecret"),
                new KeyValuePair<string, ProviderConfiguration>[0]);

            return integrationConfig;
        }
    }
}
