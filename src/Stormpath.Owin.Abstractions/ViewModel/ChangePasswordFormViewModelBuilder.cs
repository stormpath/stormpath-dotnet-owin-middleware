using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.SDK.Client;

namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class ChangePasswordFormViewModelBuilder
    {
        private readonly IClient _client;
        private readonly IntegrationConfiguration _configuration;

        public ChangePasswordFormViewModelBuilder(
            IClient client, // TODO remove when refactoring JWT
            IntegrationConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        public ChangePasswordFormViewModel Build()
        {
            var baseViewModelBuilder = new ChangePasswordViewModelBuilder(_configuration.Web);
            var result = new ChangePasswordFormViewModel(baseViewModelBuilder.Build());

            // Add a state (CSRF) token
            result.StateToken = new StateTokenBuilder(_client, _configuration.Client.ApiKey).ToString();

            return result;
        }
    }
}
