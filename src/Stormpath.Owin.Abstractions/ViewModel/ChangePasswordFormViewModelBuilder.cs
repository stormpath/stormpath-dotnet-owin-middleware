using Stormpath.Owin.Abstractions.Configuration;
namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class ChangePasswordFormViewModelBuilder
    {
        private readonly IntegrationConfiguration _configuration;

        public ChangePasswordFormViewModelBuilder(
            IntegrationConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ChangePasswordFormViewModel Build()
        {
            var baseViewModelBuilder = new ChangePasswordViewModelBuilder(_configuration.Web);
            var result = new ChangePasswordFormViewModel(baseViewModelBuilder.Build());

            // Add a state (CSRF) token
            result.StateToken = new StateTokenBuilder(_configuration.Client.ApiKey).ToString();

            return result;
        }
    }
}
