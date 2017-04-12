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

        Task<GrantResult> PostRefreshGrantAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string refreshToken,
            CancellationToken cancellationToken);

        Task<User> GetUserAsync(
            string userId,
            CancellationToken cancellationToken);

        Task<User> CreateUserAsync(
            dynamic profile,
            string password,
            string recoveryQuestion,
            string recoveryAnswer,
            CancellationToken cancellationToken);

        Task<TokenIntrospectionResult> IntrospectTokenAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string token,
            string tokenType,
            CancellationToken cancelationToken);

        Task AddUserToAppAsync(
            string appId,
            string userId,
            string email,
            CancellationToken cancellationToken);

        Task SendPasswordResetEmailAsync(
            string login,
            CancellationToken cancellationToken);

        Task<RecoveryTransactionObject> VerifyRecoveryTokenAsync(string token, CancellationToken cancellationToken);

        Task<RecoveryTransactionObject> AnswerRecoveryQuestionAsync(string stateToken, string answer, CancellationToken cancellationToken);

        Task<RecoveryTransactionObject> ResetPasswordAsync(string stateToken, string newPassword, CancellationToken cancellationToken);
    }
}