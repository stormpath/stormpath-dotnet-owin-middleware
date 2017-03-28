using Stormpath.Owin.Abstractions.Configuration;
namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class VerifyEmailFormViewModelBuilder
    {
        private readonly IntegrationConfiguration _configuration;

        public VerifyEmailFormViewModelBuilder(
            IntegrationConfiguration configuration)
        {
            _configuration = configuration;
        }

        public VerifyEmailFormViewModel Build()
        {
            var baseViewModelBuilder = new VerifyEmailViewModelBuilder(_configuration.Web);
            var result = new VerifyEmailFormViewModel(baseViewModelBuilder.Build());

            // Add a state (CSRF) token
            result.StateToken = new StateTokenBuilder(_configuration.Client.ApiKey).ToString();

            return result;
        }
    }
}
