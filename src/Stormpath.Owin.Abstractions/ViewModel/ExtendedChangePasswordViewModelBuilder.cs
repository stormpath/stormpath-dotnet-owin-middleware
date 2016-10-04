using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.SDK.Client;

namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class ExtendedChangePasswordViewModelBuilder
    {
        private readonly IClient _client;
        private readonly IntegrationConfiguration _configuration;

        public ExtendedChangePasswordViewModelBuilder(
            IClient client, // TODO remove when refactoring JWT
            IntegrationConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        public ExtendedChangePasswordViewModel Build()
        {
            var baseViewModelBuilder = new ChangePasswordViewModelBuilder(_configuration.Web);
            var result = new ExtendedChangePasswordViewModel(baseViewModelBuilder.Build());

            // Add a state (CSRF) token
            result.StateToken = new StateTokenBuilder(_client, _configuration.Client.ApiKey).ToString();

            return result;
        }
    }
}
