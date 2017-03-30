using System.Threading.Tasks;

namespace Stormpath.Owin.Middleware.Okta
{
    public interface IOktaClient
    {
        Task<Application> GetApplication(string appId);
        Task<ApplicationClientCredentials> GetClientCredentials(string appId);
        Task<GrantResult> PostPasswordGrant(string authorizationServerId, string clientId, string clientSecret, string username, string password);
    }
}