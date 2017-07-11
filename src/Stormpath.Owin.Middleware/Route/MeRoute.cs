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
                account = await ExpandAccount(stormpathAccount, _oktaClient, _configuration.Web.Me.Expand, cancellationToken)
            };

            return await JsonResponse.Ok(context, responseModel);
        }

        private static async Task<MeResponseModel> ExpandAccount(
            ICompatibleOktaAccount account,
            IOktaClient oktaClient,
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
                EmailVerificationToken = account.EmailVerificationToken
            };

            if (!expansionOptions.Any(e => e.Value))
            {
                return sanitizedModel;
            }

            if (expansionOptions.Any(e => e.Key.Equals("customData", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                sanitizedModel.CustomData = account.CustomData;
            }

            if (expansionOptions.Any(e => e.Key.Equals("groups", StringComparison.OrdinalIgnoreCase) && e.Value))
            {
                var groups = await oktaClient.GetGroupsForUserIdAsync(account.GetOktaUser().Id, cancellationToken);

                sanitizedModel.Groups = new MeGroupsCollectionModel
                {
                    Size = groups.Length,
                    Items = groups.Select(g => new MeGroupModel
                        {
                            Id = g.Id,
                            Name = g.Profile.Name,
                            Description = g.Profile.Description,
                            CreatedAt = g.Created,
                            ModifiedAt = g.LastUpdated
                        })
                        .ToArray()
                };
            }

            // TODO other expansion patches?

            return sanitizedModel;
        }
    }
}
