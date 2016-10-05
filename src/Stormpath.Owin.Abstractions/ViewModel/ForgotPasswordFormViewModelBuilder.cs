using System.Collections.Generic;
using Stormpath.Owin.Abstractions.Configuration;
using Stormpath.SDK.Client;

namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class ForgotPasswordFormViewModelBuilder
    {
        private readonly IClient _client;
        private readonly IntegrationConfiguration _configuration;
        private readonly IDictionary<string, string[]> _queryString;

        public ForgotPasswordFormViewModelBuilder(
            IClient client, // TODO remove when refactoring JWT
            IntegrationConfiguration configuration,
            IDictionary<string, string[]> queryString)
        {
            _client = client;
            _configuration = configuration;
            _queryString = queryString;
        }

        public ForgotPasswordFormViewModel Build()
        {
            var baseViewModelBuilder = new ForgotPasswordViewModelBuilder(_configuration.Web, _queryString);
            var result = new ForgotPasswordFormViewModel(baseViewModelBuilder.Build());

            // Add a state (CSRF) token
            result.StateToken = new StateTokenBuilder(_client, _configuration.Client.ApiKey).ToString();

            return result;
        }
    }
}
