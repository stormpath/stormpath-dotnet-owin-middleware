// <copyright file="LogoutRoute.cs" company="Stormpath, Inc.">
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
    public sealed class LogoutRoute : AbstractRoute
    {
        protected override async Task<bool> PostHtmlAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var executor = new LogoutExecutor(client, _configuration, _handlers, _logger);
            await executor.HandleLogoutAsync(context, cancellationToken);

            return await executor.HandleRedirectAsync(context);
        }

        protected override async Task<bool> PostJsonAsync(IOwinEnvironment context, IClient client, ContentType bodyContentType, CancellationToken cancellationToken)
        {
            var executor = new LogoutExecutor(client, _configuration, _handlers, _logger);
            await executor.HandleLogoutAsync(context, cancellationToken);

            await JsonResponse.Ok(context);
            return true;
        }
    }
}
