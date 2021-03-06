﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stormpath.Owin.Middleware.Okta
{
    public class TokenIntrospectionResult
    {
        public static TokenIntrospectionResult Invalid = new TokenIntrospectionResult { Active = false };

        public bool Active { get; set; }
        public string Scope { get; set; }
        public string Username { get; set; }
        public int? Exp { get; set; }
        public int? Iat { get; set; }
        public string Sub { get; set; }

        [JsonConverter(typeof(StringOrListConverter))]
        public IList<string> Aud { get; set; }

        public string Iss { get; set; }
        public string Jti { get; set; }
        public string Uid { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
    }
}
