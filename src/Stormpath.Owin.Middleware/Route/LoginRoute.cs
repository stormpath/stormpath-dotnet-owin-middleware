// <copyright file="LoginRoute.cs" company="Stormpath, Inc.">
// Copyright (c) 2016 Stormpath, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.Owin.Middleware.ViewModel;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Route
{
    public class LoginRoute : AbstractRouteMiddleware
    {
        private readonly static string[] SupportedMethods = { "GET", "POST" };
        private readonly static string[] SupportedContentTypes = { "text/html", "application/json" };

        public LoginRoute(
            StormpathConfiguration configuration,
            ILogger logger,
            IClient client)
            : base(configuration, logger, client, SupportedMethods, SupportedContentTypes)
        {
        }

        protected override Task GetHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            // todo
            context.Response.Headers.SetString("Content-Type", Constants.HtmlContentType);

            return context.Response.WriteAsync("Hello world", Encoding.UTF8, cancellationToken);
        }

        protected override Task GetJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var loginViewModel = BuildViewModel();

            return JsonResponse.Ok(loginViewModel, context, cancellationToken);
        }

        private LoginViewModel BuildViewModel()
        {
            var result = new LoginViewModel();

            foreach (var fieldName in _configuration.Web.Login.Form.FieldOrder)
            {
                Configuration.Abstractions.Model.WebFieldConfiguration field = null;
                if (!_configuration.Web.Login.Form.Fields.TryGetValue(fieldName, out field))
                {
                    throw new Exception($"Invalid field '{fieldName}' in fieldOrder list.");
                }

                result.Form.Fields.Add(new LoginFormFieldViewModel()
                {
                    Label = field.Label,
                    Name = fieldName,
                    Placeholder = field.Placeholder,
                    Required = field.Required,
                    Type = field.Type
                });
            }

            return result;
        }
    }
}
