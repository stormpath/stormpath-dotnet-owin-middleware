// <copyright file="Serializer.cs" company="Stormpath, Inc.">
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Stormpath.Owin.Middleware.Internal
{
    internal static class Serializer
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static string Serialize(object @obj)
        {
            var serialized = JsonConvert.SerializeObject(@obj, Formatting.Indented, settings);
            return serialized;
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static IDictionary<string, object> DeserializeDictionary(string json)
        {
            var deserializedMap = (JObject)JsonConvert.DeserializeObject(json, settings);
            var sanitizedMap = Sanitize(deserializedMap);

            return sanitizedMap;
        }

        /// <summary>
        /// Converts a nested tree of <see cref="JObject"/> instances into nested <see cref="IDictionary{TKey, TValue}">dictionaries</see>.
        /// </summary>
        /// <remarks>JSON.NET deserializes everything into nested JObjects. We want IDictionaries all the way down.</remarks>
        /// <param name="map">Deserialized <see cref="JObject"/> from JSON.NET</param>
        /// <returns><see cref="IDictionary{TKey, TValue}"/> of primitive items, and embedded objects as nested <see cref="IDictionary{TKey, TValue}"/></returns>
        private static IDictionary<string, object> Sanitize(JObject map)
        {
            var result = new Dictionary<string, object>();

            if (map == null)
            {
                return result;
            }

            foreach (var prop in map.Properties())
            {
                var name = prop.Name;
                var value = SanitizeToken(prop.Value);

                result.Add(name, value);
            }

            return result;
        }

        private static object SanitizeToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                    if (token.Children().Any() &&
                        token.Children().All(t => t.Type == JTokenType.Object))
                    {
                        // Collections of sub-objects get recursively sanitized
                        var nestedObjects = token.Children()
                            .Select(c => Sanitize((JObject)c))
                            .ToList();
                        return nestedObjects;
                    }
                    else
                    {
                        var nestedScalars = token.Children()
                            .Select(c => SanitizeToken(c))
                            .ToList();
                        return nestedScalars;
                    }

                case JTokenType.Object:
                    return Sanitize((JObject)token);

                case JTokenType.Date:
                    return token.ToObject<DateTimeOffset>();

                case JTokenType.Integer:
                    var raw = token.ToString();
                    int intResult;
                    long longResult;

                    if (int.TryParse(raw, out intResult))
                    {
                        return intResult;
                    }
                    else if (long.TryParse(raw, out longResult))
                    {
                        return longResult;
                    }
                    else
                    {
                        return raw;
                    }

                case JTokenType.Boolean:
                    return bool.Parse(token.ToString());

                case JTokenType.Null:
                    return null;

                default:
                    return token.ToString();
            }
        }
    }
}
