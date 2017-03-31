using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Abstractions.ViewModel;

namespace Stormpath.Owin.Middleware.ViewModelBuilder
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
            result.StateToken = new StateTokenBuilder(_configuration.OktaEnvironment.ClientSecret).ToString();

            return result;
        }
    }
}
