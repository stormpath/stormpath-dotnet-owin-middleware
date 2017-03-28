// <copyright file="StormpathTokenExchanger.cs" company="Stormpath, Inc.">
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
using Microsoft.Extensions.Logging;
using Stormpath.Configuration.Abstractions.Immutable;

namespace Stormpath.Owin.Middleware.Internal
{
    public sealed class StormpathTokenExchanger
    {
        private readonly StormpathConfiguration _configuration;
        private readonly ILogger _logger;

        public StormpathTokenExchanger(
            StormpathConfiguration configuration,
            ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task<GrantResult> ExchangeAsync(CancellationToken cancellationToken)
        {
            //var oauthExchangeJwt = _client.NewJwtBuilder()
            //    .SetSubject(account.Href)
            //    .SetIssuedAt(DateTimeOffset.UtcNow.AddSeconds(-5))
            //    .SetExpiration(DateTimeOffset.UtcNow.AddMinutes(1)) // very short
            //    .SetIssuer(_application.Href)
            //    .SetClaim("status", "AUTHENTICATED")
            //    .SetAudience(_configuration.Client.ApiKey.Id)
            //    .SignWith(_configuration.Client.ApiKey.Secret, Encoding.UTF8)
            //    .Build();

            //var exchangeRequest = OauthRequests.NewIdSiteTokenAuthenticationRequest()
            //    .SetJwt(oauthExchangeJwt.ToString())
            //    .Build();

            //try
            //{
            //    return _application
            //        .NewIdSiteTokenAuthenticator()
            //        .AuthenticateAsync(exchangeRequest, cancellationToken);
            //}
            //catch (ResourceException rex)
            //{
            //    _logger.LogWarning(rex, source: nameof(StormpathTokenExchanger));

            //    return Task.FromResult<GrantResult>(null);
            //}
            // todo remove
            throw new Exception();
        }
    }
}
