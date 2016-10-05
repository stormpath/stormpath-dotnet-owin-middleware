using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.SDK.Client;

namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class ExtendedVerifyEmailViewModelBuilder
    {
        private readonly IClient _client;
        private readonly IntegrationConfiguration _configuration;

        public ExtendedVerifyEmailViewModelBuilder(
            IClient client, // TODO remove when refactoring JWT
            IntegrationConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        public ExtendedVerifyEmailViewModel Build()
        {
            var baseViewModelBuilder = new VerifyEmailViewModelBuilder(_configuration.Web);
            var result = new ExtendedVerifyEmailViewModel(baseViewModelBuilder.Build());

            // Add a state (CSRF) token
            result.StateToken = new StateTokenBuilder(_client, _configuration.Client.ApiKey).ToString();

            return result;
        }
    }
}
