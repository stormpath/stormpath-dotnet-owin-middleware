// <copyright file="IdSiteRedirectRoute.cs" company="Stormpath, Inc.">
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

using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.SDK.Client;

namespace Stormpath.Owin.Middleware.Route
{
    public class IdSiteRedirectRoute : AbstractRoute
    {
        protected override async Task<bool> GetAsync(
            IOwinEnvironment context,
            IClient client, 
            ContentNegotiationResult contentNegotiationResult,
            CancellationToken cancellationToken)
        {
            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);
            var options = _options as IdSiteRedirectOptions ?? new IdSiteRedirectOptions();

            var queryString = QueryStringParser.Parse(context.Request.QueryString, _logger);
            var stateToken = queryString.GetString(StringConstants.StateTokenName);

            if (string.IsNullOrEmpty(stateToken) || !new StateTokenParser(client, _configuration.Client.ApiKey, stateToken, _logger).Valid)
            {
                stateToken = new StateTokenBuilder(client, _configuration.Client.ApiKey).ToString();
            }

            var idSiteUrlBuilder = application.NewIdSiteUrlBuilder()
                .SetCallbackUri(options.CallbackUri)
                .SetPath(options.Path)
                .SetState(stateToken);

            if (options.Logout)
            {
                idSiteUrlBuilder.ForLogout();
            }

            var idSiteUrl = idSiteUrlBuilder.Build();

            return await HttpResponse.Redirect(context, idSiteUrl);
        }
    }
}