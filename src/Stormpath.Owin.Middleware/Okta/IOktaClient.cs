using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware.Okta
{
    public interface IOktaClient
    {
        Task<Application> GetApplicationAsync(
            string appId,
            CancellationToken cancellationToken);

        Task<ApplicationClientCredentials> GetClientCredentialsAsync(
            string appId,
            CancellationToken cancellationToken);

        Task<GrantResult> PostPasswordGrantAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret, 
            string username, 
            string password,
            CancellationToken cancellationToken);

        Task<User> GetUserAsync(
            string userId,
            CancellationToken cancellationToken);

        Task<TokenIntrospectionResult> IntrospectTokenAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string token,
            string tokenType,
            CancellationToken cancelationToken);
    }
}