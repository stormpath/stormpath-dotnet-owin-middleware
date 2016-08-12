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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Owin.Abstractions;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.SDK;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Resource;

namespace Stormpath.Owin.Middleware.Route
{
    public class MeRoute : AbstractRoute
    {
        protected override async Task<bool> GetJsonAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            Caching.AddDoNotCacheHeaders(context);

            var stormpathAccount = context.Request[OwinKeys.StormpathUser] as IAccount;

            var expansionOptions = _configuration.Web.Me.Expand;
            if (expansionOptions.Any(e => e.Value))
            {
                stormpathAccount = await GetExpandedAccount(stormpathAccount.Href, client, expansionOptions, cancellationToken);
            }

            var responseModel = new
            {
                account = await SanitizeExpandedAccount(stormpathAccount, expansionOptions, cancellationToken)
            };

            return await JsonResponse.Ok(context, responseModel);
        }

        private static async Task<IAccount> GetExpandedAccount(
            string href,
            IClient client,
            IReadOnlyDictionary<string, bool> expansionOptions,
            CancellationToken cancellationToken)
        {
            var options = new Action<IRetrievalOptions<IAccount>>(opt =>
            {
                if (expansionOptions.Any(e => e.Key.Equals("applications", StringComparison.OrdinalIgnoreCase) && e.Value))
                {
                    opt.Expand(acct => acct.GetApplications());
                }

                if (expansionOptions.Any(e => e.Key.Equals("customData", StringComparison.OrdinalIgnoreCase) && e.Value))
                {
                    opt.Expand(acct => acct.GetCustomData());
                }

                if (expansionOptions.Any(e => e.Key.Equals("directory", StringComparison.OrdinalIgnoreCase) && e.Value))
                {
                    opt.Expand(acct => acct.GetDirectory());
                }

                if (expansionOptions.Any(e => e.Key.Equals("groupMemberships", StringComparison.OrdinalIgnoreCase) && e.Value))
                {
                    opt.Expand(acct => acct.GetGroupMemberships());
                }

                if (expansionOptions.Any(e => e.Key.Equals("groups", StringComparison.OrdinalIgnoreCase) && e.Value))
                {
                    opt.Expand(acct => acct.GetGroups());
                }

                if (expansionOptions.Any(e => e.Key.Equals("providerData", StringComparison.OrdinalIgnoreCase) && e.Value))
                {
                    opt.Expand(acct => acct.GetProviderData());
                }

                if (expansionOptions.Any(e => e.Key.Equals("tenant", StringComparison.OrdinalIgnoreCase) && e.Value))
                {
                    opt.Expand(acct => acct.GetTenant());
                }
            });

            return await client.GetAccountAsync(href, options, cancellationToken);
        }

        private static async Task<AccountResponseModel> SanitizeExpandedAccount(
            IAccount account,
            IReadOnlyDictionary<string, bool> expansionOptions,
            CancellationToken cancellationToken)
        {
            var sanitizedModel = new AccountResponseModel
            {
                Href = account.Href,
                Email = account.Email,
                GivenName = account.GivenName,
                MiddleName = account.MiddleName,
                Surname = account.Surname,
                FullName = account.FullName,
                CreatedAt = account.CreatedAt,
                ModifiedAt = account.ModifiedAt,
                Status = account.Status.ToString()
            };

            if (expansionOptions.Any(e => e.Key.Equals("applications", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                // TODO replace with ToArrayAsync
                sanitizedModel.Applications = await account.GetApplications().ToListAsync(cancellationToken);
            }

            if (expansionOptions.Any(e => e.Key.Equals("customData", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                sanitizedModel.CustomData = await account.GetCustomDataAsync(cancellationToken);
            }

            if (expansionOptions.Any(e => e.Key.Equals("directory", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                sanitizedModel.Directory = await account.GetDirectoryAsync(cancellationToken);
            }

            if (expansionOptions.Any(e => e.Key.Equals("groupMemberships", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                // TODO replace with ToArrayAsync
                sanitizedModel.GroupMemberships = await account.GetGroupMemberships().ToListAsync(cancellationToken);
            }

            if (expansionOptions.Any(e => e.Key.Equals("groups", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                // TODO replace with ToArrayAsync
                sanitizedModel.Groups = await account.GetGroups().ToListAsync(cancellationToken);
            }

            if (expansionOptions.Any(e => e.Key.Equals("providerData", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                sanitizedModel.ProviderData = await account.GetProviderDataAsync(cancellationToken);
            }

            if (expansionOptions.Any(e => e.Key.Equals("tenant", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                sanitizedModel.Tenant = await account.GetTenantAsync(cancellationToken);
            }

            return sanitizedModel;
        }
    }
}
