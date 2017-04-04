using System.Collections.Generic;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.Owin.Abstractions.ViewModel;

namespace Stormpath.Owin.Middleware.ViewModelBuilder
{
    public sealed class ForgotPasswordFormViewModelBuilder
    {
        private readonly IntegrationConfiguration _configuration;
        private readonly IDictionary<string, string[]> _queryString;

        public ForgotPasswordFormViewModelBuilder(
            IntegrationConfiguration configuration,
            IDictionary<string, string[]> queryString)
        {
            _configuration = configuration;
            _queryString = queryString;
        }

        public ForgotPasswordFormViewModel Build()
        {
            var baseViewModelBuilder = new ForgotPasswordViewModelBuilder(_configuration.Web, _queryString);
            var result = new ForgotPasswordFormViewModel(baseViewModelBuilder.Build());

            // Add a state (CSRF) token
            result.StateToken = new StateTokenBuilder(_configuration.Okta.Application.Id, _configuration.OktaEnvironment.ClientSecret).ToString();

            return result;
        }
    }
}
