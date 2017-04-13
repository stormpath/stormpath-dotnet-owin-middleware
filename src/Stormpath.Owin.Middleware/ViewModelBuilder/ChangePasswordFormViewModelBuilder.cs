using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Abstractions.ViewModel;

namespace Stormpath.Owin.Middleware.ViewModelBuilder
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
            result.StateToken = new StateTokenBuilder(_configuration.Application.Id, _configuration.OktaEnvironment.ClientSecret).ToString();

            return result;
        }
    }
}
