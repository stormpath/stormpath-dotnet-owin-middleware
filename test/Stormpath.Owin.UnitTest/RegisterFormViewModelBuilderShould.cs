using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Abstractions.ViewModel;
using Stormpath.SDK.Client;
using Xunit;

namespace Stormpath.Owin.UnitTest
{
    public class RegisterFormViewModelBuilderShould
    {
        /// <summary>
        /// Regression test for https://github.com/stormpath/stormpath-dotnet-owin-middleware/issues/69
        /// </summary>
        [Fact]
        public void NotThrowForMissingFieldTypeOnFormResubmission()
        {
            var client = Substitute.For<IClient>();
            var config = new StormpathConfiguration()
            {
                Web = new WebConfiguration
                {
                    Register = new WebRegisterRouteConfiguration
                    {
                        Form = new WebRegisterRouteFormConfiguration
                        {
                            Fields = new Dictionary<string, WebFieldConfiguration>
                            {
                                ["CustomFieldsRock"] = new WebFieldConfiguration { Required = true }
                            }
                        }
                    }
                }
            };

            var previousFormData = new Dictionary<string, string[]>()
            {
                ["st"] = new [] {"blah"},
                ["givenName"] = new [] {"Galen"},
                ["surname"] = new[] {"Erso"},
                ["CustomFieldsRock"] = new [] {"indeed"}
            };

            var viewModelBuilder = new RegisterFormViewModelBuilder(
                client,
                ConfigurationHelper.CreateFakeConfiguration(config),
                new Dictionary<string, string[]>(),
                previousFormData,
                logger: null);

            var result = viewModelBuilder.Build();
            result.Form.Fields.Should().Contain(x => x.Name == "CustomFieldsRock" && x.Required);
        }
    }
}
