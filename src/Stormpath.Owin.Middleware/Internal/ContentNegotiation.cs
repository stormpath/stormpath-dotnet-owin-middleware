// <copyright file="ContentNegotiation.cs" company="Stormpath, Inc.">
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

namespace Stormpath.Owin.Middleware.Internal
{
    public static class ContentNegotiation
    {
        public static ContentNegotiationResult NegotiateAcceptHeader(string acceptHeader, IEnumerable<string> producesList)
        {
            if (string.IsNullOrEmpty(acceptHeader))
            {
                acceptHeader = "*/*";
            }

            var sortedAccept = ParseAndSortHeader(acceptHeader);

            foreach (var potentialContentType in sortedAccept)
            {
                if (potentialContentType == ContentType.Any)
                {
                    return new ContentNegotiationResult(
                        success: true,
                        contentType: ContentType.Parse(producesList.First()));
                }

                if (producesList.Contains(potentialContentType))
                {
                    return new ContentNegotiationResult(
                        success: true,
                        contentType: ContentType.Parse(potentialContentType));
                }
            }

            // Negotiation failed
            return new ContentNegotiationResult(
                success: false,
                contentType: ContentType.Parse(sortedAccept.First()));
        }

        public static ContentNegotiationResult DetectBodyType(string bodyContentType)
        {
            var parsed = ContentType.Parse(bodyContentType);
            var isValid = parsed == ContentType.FormUrlEncoded || parsed == ContentType.Json;

            return new ContentNegotiationResult(isValid, parsed);
        }

        private static string[] ParseAndSortHeader(string acceptHeader)
        {
            var results = new Dictionary<string, double>();

            var mediaRanges = acceptHeader.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var mediaRange in mediaRanges)
            {
                var tokens = mediaRange
                    .Split(';')
                    .Select(t => t.Trim());

                var qualityFactor = 1.0;
                var qualityFactorToken = tokens.Where(t => t.StartsWith("q=")).SingleOrDefault();

                if (!string.IsNullOrEmpty(qualityFactorToken))
                {
                    qualityFactor = double.Parse(qualityFactorToken.Substring(qualityFactorToken.IndexOf("q=") + 2));
                }

                results.Add(tokens.ElementAt(0), qualityFactor);
            }

            var sorted = results
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key);

            return sorted.ToArray();
        }
    }
}
