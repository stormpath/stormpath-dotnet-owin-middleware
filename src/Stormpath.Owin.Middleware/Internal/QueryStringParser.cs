// <copyright file="Error.cs" company="Stormpath, Inc.">
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

using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Stormpath.Owin.Middleware.Internal
{
    public static class QueryStringParser
    {
        public static IDictionary<string, string[]> Parse(string queryString)
        {
            var temporaryDictionary = new Dictionary<string, List<string>>();

            foreach (var item in queryString.Split('&'))
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                var tokens = item.Split('=');
                var key = WebUtility.UrlDecode(tokens[0]);
                var value = WebUtility.UrlDecode(tokens[1]);

                if (!temporaryDictionary.ContainsKey(key))
                {
                    temporaryDictionary[key] = new List<string>();
                }

                temporaryDictionary[key].Add(value);
            }

            return temporaryDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
        }
    }
}
