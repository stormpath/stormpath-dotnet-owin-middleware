using System.Collections.Generic;
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

        Task<GrantResult> PostAuthCodeGrantAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string code,
            string originalRedirectUri,
            CancellationToken cancellationToken);

        Task<User> GetUserAsync(
            string userId,
            CancellationToken cancellationToken);

        Task<List<User>> FindUsersByEmailAsync(
            string email,
            CancellationToken cancellationToken);

        Task<List<User>> SearchUsersAsync(
            string searchExpression,
            CancellationToken cancellationToken);

        Task<User> CreateUserAsync(
            IDictionary<string, object> profile,
            string password,
            bool activate,
            string recoveryQuestion,
            string recoveryAnswer,
            CancellationToken cancellationToken);

        Task<User> UpdateUserProfileAsync(
            string userId,
            IDictionary<string, object> updatedProfileProperties,
            CancellationToken cancellationToken);

        Task ActivateUserAsync(
            string userId,
            CancellationToken cancellationToken);

        Task<TokenIntrospectionResult> IntrospectTokenAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string token,
            string tokenType,
            CancellationToken cancelationToken);

        Task RevokeTokenAsync(
            string authorizationServerId,
            string clientId,
            string clientSecret,
            string token,
            string tokenType,
            CancellationToken cancellationToken);

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

        Task<IdentityProvider[]> GetIdentityProvidersAsync(CancellationToken ct);

        Task<Group[]> GetGroupsForUserIdAsync(string userId, CancellationToken cancellationToken);

        Task<ShimApiKey> GetApiKeyAsync(string apiKeyId, CancellationToken cancellationToken);

        Task<AuthorizationServer> GetAuthorizationServerAsync(string authorizationServerId, CancellationToken cancellationToken);
    }
}