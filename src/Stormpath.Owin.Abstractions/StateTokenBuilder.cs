using System;

namespace Stormpath.Owin.Abstractions
{
    public sealed class StateTokenBuilder
    {
        public const string PathClaimName = "path";
        public const string StateClaimName = "state";

        // TODO: replace with direct JWT library usage

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
            // TODO: replace with direct JWT library usage
            throw new Exception("TODO");

            //if (!string.IsNullOrEmpty(Path))
            //{
            //    jwtBuilder.SetClaim("path", Path);
            //}

            //if (!string.IsNullOrEmpty(State))
            //{
            //    jwtBuilder.SetClaim("state", State);
            //}
            
            //jwtBuilder.SetExpiration(DateTimeOffset.Now.Add(ExpiresIn));
            //jwtBuilder.SignWith(_apiKeyConfiguration.Secret, Encoding.UTF8);

            //return jwtBuilder.Build().ToString();
        }
    }
}
