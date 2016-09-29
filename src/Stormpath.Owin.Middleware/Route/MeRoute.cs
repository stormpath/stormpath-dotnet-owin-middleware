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
using System.Security.Cryptography;
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
    public sealed class MeRoute : AbstractRoute
    {
        protected override Task<bool> GetHtmlAsync(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            return GetJsonAsync(context, client, cancellationToken);
        }

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
                // TODO retrieve collections when collection caching is in SDK: applications, groupMemberships, groups
                if (expansionOptions.Any(e => e.Key.Equals("customData", StringComparison.OrdinalIgnoreCase) && e.Value))
                {
                    opt.Expand(acct => acct.GetCustomData());
                }

                if (expansionOptions.Any(e => e.Key.Equals("directory", StringComparison.OrdinalIgnoreCase) && e.Value))
                {
                    opt.Expand(acct => acct.GetDirectory());
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

        private static async Task<MeResponseModel> SanitizeExpandedAccount(
            IAccount account,
            IReadOnlyDictionary<string, bool> expansionOptions,
            CancellationToken cancellationToken)
        {
            var sanitizedModel = new MeResponseModel
            {
                Href = account.Href,
                Email = account.Email,
                Username = account.Username,
                GivenName = account.GivenName,
                MiddleName = account.MiddleName,
                Surname = account.Surname,
                FullName = account.FullName,
                CreatedAt = account.CreatedAt,
                ModifiedAt = account.ModifiedAt,
                Status = account.Status.ToString(),
                PasswordModifiedAt = account.PasswordModifiedAt,
                EmailVerificationToken = account.EmailVerificationToken?.GetValue()
            };

            if (!expansionOptions.Any(e => e.Value))
            {
                return sanitizedModel;
            }

            // TODO might be able to simply return the interface objects directly after explicit interfaces are removed

            if (expansionOptions.Any(e => e.Key.Equals("applications", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                // TODO replace with ToArrayAsync
                var applications = await account.GetApplications().ToListAsync(cancellationToken);
                sanitizedModel.Applications = new
                {
                    size = applications.Count,
                    items = applications.Select(a => new
                    {
                        href = a.Href,
                        name = a.Name,
                        description = a.Description,
                        status = a.Status.ToString(),
                        createdAt = a.CreatedAt,
                        modifiedAt = a.ModifiedAt,
                    })
                };
            }

            if (expansionOptions.Any(e => e.Key.Equals("apiKeys", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                // TODO replace with ToArrayAsync
                var apiKeys = await account.GetApiKeys().ToListAsync(cancellationToken);
                sanitizedModel.ApiKeys = new
                {
                    size = apiKeys.Count,
                    items = apiKeys.Select(k => new
                    {
                        href = k.Href,
                        name = k.Name,
                        description = k.Description,
                        id = k.Id,
                        status = k.Status.ToString(),
                    })
                };
            }

            if (expansionOptions.Any(e => e.Key.Equals("customData", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                sanitizedModel.CustomData = (await account.GetCustomDataAsync(cancellationToken))
                    .ToDictionary(cd => cd.Key, cd => cd.Value);
            }

            if (expansionOptions.Any(e => e.Key.Equals("directory", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                var directory = await account.GetDirectoryAsync(cancellationToken);
                sanitizedModel.Directory = new
                {
                    href = directory.Href,
                    name = directory.Name,
                    description = directory.Description,
                    status = directory.Status.ToString(),
                    createdAt = directory.CreatedAt,
                    modifiedAt = directory.ModifiedAt,
                };
            }

            if (expansionOptions.Any(e => e.Key.Equals("groupMemberships", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                // TODO replace with ToArrayAsync
                var groupMemberships = await account.GetGroupMemberships().ToListAsync(cancellationToken);
                sanitizedModel.GroupMemberships = new
                {
                    size = groupMemberships.Count,
                    items = groupMemberships.Select(gm => new
                    {
                        href = gm.Href
                    })
                };
            }

            if (expansionOptions.Any(e => e.Key.Equals("groups", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                // TODO replace with ToArrayAsync
                var groups = await account.GetGroups().ToListAsync(cancellationToken);
                sanitizedModel.Groups = new
                {
                    // TODO add href if available on IAsyncQueryable
                    size = groups.Count,
                    items = groups.Select(g => new
                    {
                        href = g.Href,
                        name = g.Name,
                        description = g.Description,
                        status = g.Status.ToString(),
                        createdAt = g.CreatedAt,
                        modifiedAt = g.ModifiedAt,
                    })
                };
            }

            if (expansionOptions.Any(e => e.Key.Equals("providerData", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                var providerData = await account.GetProviderDataAsync(cancellationToken);
                sanitizedModel.ProviderData = new
                {
                    href = providerData.Href,
                    createdAt = providerData.CreatedAt,
                    modifiedAt = providerData.ModifiedAt,
                    providerId = providerData.ProviderId
                };
            }

            if (expansionOptions.Any(e => e.Key.Equals("tenant", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                var tenant = await account.GetTenantAsync(cancellationToken);
                sanitizedModel.Tenant = new
                {
                    href = tenant.Href,
                    name = tenant.Name,
                    key = tenant.Key,
                    createdAt = tenant.CreatedAt,
                    modifiedAt = tenant.ModifiedAt
                };
            }

            return sanitizedModel;
        }
    }
}
