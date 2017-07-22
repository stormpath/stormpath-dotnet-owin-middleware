// <copyright file="AccountStoreViewModel.cs" company="Stormpath, Inc.">
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

namespace Stormpath.Owin.Abstractions.ViewModel
{
    public sealed class AccountStoreViewModel
    {
        public string Href { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public static string CreateUriFromTemplate(
            string template,
            string clientId,
            string responseType,
            string responseMode,
            string scopes,
            string redirectUri,
            string state,
            string nonce)
        {
            var templated = template
                .Replace("{clientId}", clientId)
                .Replace("{responseType}", responseType)
                .Replace("{responseMode}", responseMode)
                .Replace("{scopes}", scopes)
                .Replace("{redirectUri}", redirectUri)
                .Replace("{state}", state)
                .Replace("{nonce}", nonce);

            return $"{templated}";
        }
    }
}
