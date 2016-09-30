using System;
using System.Text;
using Stormpath.Configuration.Abstractions.Immutable;
using Stormpath.SDK.Client;

namespace Stormpath.Owin.Abstractions
{
    public sealed class StateTokenBuilder
    {
        public const string PathClaimName = "path";
        public const string StateClaimName = "state";

        // TODO: replace with direct JWT library usage
        private readonly IClient _client;

        private readonly ClientApiKeyConfiguration _apiKeyConfiguration;

        public StateTokenBuilder(IClient client, ClientApiKeyConfiguration apiKeyConfiguration)
        {
            _client = client;
            _apiKeyConfiguration = apiKeyConfiguration;
        }

        public string Path { get; set; }

        public string State { get; set; } = Guid.NewGuid().ToString();

        public TimeSpan ExpiresIn { get; set; } = TimeSpan.FromMinutes(30);

        public override string ToString()
        {
            // TODO: replace with direct JWT library usage
            var jwtBuilder = _client.NewJwtBuilder();

            if (!string.IsNullOrEmpty(Path))
            {
                jwtBuilder.SetClaim("path", Path);
            }

            if (!string.IsNullOrEmpty(State))
            {
                jwtBuilder.SetClaim("state", State);
            }
            
            jwtBuilder.SetExpiration(DateTimeOffset.Now.Add(ExpiresIn));
            jwtBuilder.SignWith(_apiKeyConfiguration.Secret, Encoding.UTF8);

            return jwtBuilder.Build().ToString();
        }
    }
}
