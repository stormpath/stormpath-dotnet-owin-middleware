// <copyright file="GithubCallbackRoute.cs" company="Stormpath, Inc.">
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Internal
{
    public sealed class OauthCodeExchanger
    {
        private readonly string _oauthUri;
        private readonly ILogger _logger;

        public OauthCodeExchanger(string oauthUri, ILogger logger)
        {
            _oauthUri = oauthUri;
            _logger = logger;
        }

        public async Task<string> ExchangeCodeForAccessTokenAsync(
            string code,
            string callbackUri,
            string clientId,
            string clientSecret,
            string stateToken,
            CancellationToken cancellationToken)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _oauthUri)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["redirect_uri"] = callbackUri,
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["state"] = stateToken
                }),
                Headers =
                {
                    Accept =
                    {
                        new MediaTypeWithQualityHeaderValue("application/json")
                    }
                }
            };

            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await new HttpClient()
                    .SendAsync(httpRequest, cancellationToken);

                httpResponse.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, source: nameof(ExchangeCodeForAccessTokenAsync));
                return null;
            }

            if (!httpResponse.Content.Headers.ContentType.MediaType.Equals("application/json"))
            {
                return null;
            }

            var httpResponseBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                await httpResponse.Content.ReadAsStringAsync());

            string error;
            if (httpResponseBody.TryGetValue("error", out error))
            {
                _logger.Warn($"OAuth error: '{error}'", nameof(ExchangeCodeForAccessTokenAsync));
                return null;
            }

            string accessToken;
            httpResponseBody.TryGetValue("access_token", out accessToken);
            return accessToken;
        }
    }
}
