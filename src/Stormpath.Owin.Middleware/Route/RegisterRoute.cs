// <copyright file="RegisterRoute.cs" company="Stormpath, Inc.">
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.Configuration.Abstractions;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Logging;
using Stormpath.Owin.Middleware.Model;

namespace Stormpath.Owin.Middleware.Route
{
    public sealed class RegisterRoute : AbstractRouteMiddleware
    {
        private readonly static string[] SupportedMethods = { "POST" };
        private readonly static string[] SupportedContentTypes = { "application/json" }; // todo

        public RegisterRoute(
            StormpathConfiguration configuration,
            ILogger logger,
            IClient client)
            : base(configuration, logger, client, SupportedMethods, SupportedContentTypes)
        {
        }

        protected override async Task PostJson(IOwinEnvironment context, IClient scopedClient, CancellationToken cancellationToken)
        {
            var bodyString = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var body = Serializer.Deserialize<RegisterPostModel>(bodyString);

            var email = body?.Email;
            var password = body?.Password;

            bool missingEmailOrPassword = string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password);
            if (missingEmailOrPassword)
            {
                throw new Exception("Missing email or password!");
            }

            var givenName = body?.GivenName ?? "UNKNOWN";
            var middleName = body?.MiddleName;
            var surname = body?.Surname ?? "UNKNOWN";
            var username = body?.Username;

            var application = await scopedClient.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            var newAccount = scopedClient.Instantiate<IAccount>()
                .SetEmail(email)
                .SetPassword(password)
                .SetGivenName(givenName)
                .SetSurname(surname);

            if (!string.IsNullOrEmpty(username))
            {
                newAccount.SetUsername(username);
            }

            if (!string.IsNullOrEmpty(middleName))
            {
                newAccount.SetMiddleName(middleName);
            }

            await application.CreateAccountAsync(newAccount, cancellationToken);

            var sanitizer = new ResponseSanitizer<IAccount>();
            var responseModel = new
            {
                account = sanitizer.Sanitize(newAccount)
            };

            await JsonResponse.Ok(context, responseModel);
        }
    }
}
