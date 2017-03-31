using System;
using System.Collections.Generic;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace Stormpath.Owin.Middleware
{
    public sealed class StateTokenBuilder
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public const string PathClaimName = "path";
        public const string StateClaimName = "state";
        public const string ExpClaimName = "exp";

        private readonly string _secret;

        public StateTokenBuilder(string secret)
        {
            _secret = secret;
        }

        public string Path { get; set; }

        public string State { get; set; } = Guid.NewGuid().ToString();

        public TimeSpan ExpiresIn { get; set; } = TimeSpan.FromMinutes(30);

        public override string ToString()
        {
            var now = new UtcDateTimeProvider().GetNow();
            var expiry = now.Add(ExpiresIn);
            var expirySecondsSinceEpoch = Math.Round((expiry - UnixEpoch).TotalSeconds);

            var payload = new Dictionary<string, object>
            {
                [ExpClaimName] = expirySecondsSinceEpoch
            };

            if (!string.IsNullOrEmpty(Path))
            {
                payload.Add(PathClaimName, Path);
            }

            if (!string.IsNullOrEmpty(State))
            {
                payload.Add(StateClaimName, State);
            }

            var encoder = new JwtEncoder(new HMACSHA256Algorithm(), new JsonNetSerializer());
            return encoder.Encode(payload, _secret);
        }
    }
}
