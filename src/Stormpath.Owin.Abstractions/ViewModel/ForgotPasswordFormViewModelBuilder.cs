using System;
using System.Collections.Generic;
using Stormpath.Owin.Abstractions.Configuration;
namespace Stormpath.Owin.Abstractions.ViewModel
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
            // Add a state (CSRF) token
            throw new NotImplementedException("TODO");

            //result.StateToken = new StateTokenBuilder(_configuration.Client.ApiKey).ToString();

            //return result;
        }
    }
}
