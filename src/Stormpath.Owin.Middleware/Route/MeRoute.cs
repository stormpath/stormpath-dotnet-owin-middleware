// <copyright file="MeRoute.cs" company="Stormpath, Inc.">
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
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Okta;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class MeRoute : AbstractRoute
    {
        protected override Task<bool> GetHtmlAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            return GetJsonAsync(context, cancellationToken);
        }

        protected override async Task<bool> GetJsonAsync(IOwinEnvironment context, CancellationToken cancellationToken)
        {
            Caching.AddDoNotCacheHeaders(context);

            var stormpathAccount = context.Request[OwinKeys.StormpathUser] as ICompatibleOktaAccount;

            var responseModel = new
            {
                account = await SanitizeExpandedAccount(stormpathAccount, cancellationToken)
            };

            return await JsonResponse.Ok(context, responseModel);
        }

        private static Task<MeResponseModel> SanitizeExpandedAccount(
            ICompatibleOktaAccount account,
            CancellationToken cancellationToken)
        {
            // todo how does me work with an idToken?
            throw new Exception("TODO");
        }
    }
}
