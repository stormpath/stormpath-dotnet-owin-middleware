// <copyright file="StormpathMiddleware.GetUser.cs" company="Stormpath, Inc.">
// Copyright (c) 2016 Stormpath, Inc.
// Portions copyright 2013 Microsoft Open Technologies, Inc. Licensed under Apache 2.0.
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
using Stormpath.SDK.Logging;

namespace Stormpath.Owin.Middleware.Internal
{
    public class CookieParser
    {
        private readonly ILogger logger;
        private readonly IDictionary<string, string> cookies;

        public CookieParser(string cookieHeader, ILogger logger)
        {
            this.logger = logger;
            this.cookies = new Dictionary<string, string>(StringComparer.Ordinal);

            ParseDelimited(cookieHeader, SemicolonAndComma, AddCookieCallback, this.cookies, logger);
        }

        public string Get(string key)
        {
            var value = string.Empty;
            this.cookies.TryGetValue(key, out value);
            return value;
        }

        private static readonly char[] SemicolonAndComma = new[] { ';', ',' };

        private static readonly Action<string, string, IDictionary<string, string>> AddCookieCallback = (name, value, dict) =>
        {
            if (!dict.ContainsKey(name))
            {
                dict.Add(name, value);
            }
        };

        private static void ParseDelimited(string text, char[] delimiters, Action<string, string, IDictionary<string, string>> callback, IDictionary<string, string> state, ILogger logger)
        {
            var textLength = text.Length;
            var equalIndex = text.IndexOf('=');
            if (equalIndex == -1)
            {
                equalIndex = textLength;
            }
            var scanIndex = 0;
            while (scanIndex < textLength)
            {
                var delimiterIndex = text.IndexOfAny(delimiters, scanIndex);
                if (delimiterIndex == -1)
                {
                    delimiterIndex = textLength;
                }
                if (equalIndex < delimiterIndex)
                {
                    while (scanIndex != equalIndex && char.IsWhiteSpace(text[scanIndex]))
                    {
                        ++scanIndex;
                    }

                    try
                    {
                        var name = text.Substring(scanIndex, equalIndex - scanIndex);
                        var value = text.Substring(equalIndex + 1, delimiterIndex - equalIndex - 1);
                        callback(
                            Uri.UnescapeDataString(name),
                            Uri.UnescapeDataString(value),
                            state);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Error parsing cookie content '{text}'", "CookieParser.ParseDelimited");
                    }

                    equalIndex = text.IndexOf('=', equalIndex + 1);
                    if (equalIndex == -1)
                    {
                        equalIndex = textLength;
                    }
                }
                scanIndex = delimiterIndex + 1;
            }
        }
    }
}
