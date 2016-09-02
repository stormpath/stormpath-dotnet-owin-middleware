using System.Threading;
using System.Threading.Tasks;
using Stormpath.SDK.Account;

namespace Stormpath.Owin.Abstractions
{
    public interface IAuthorizationFilter
    {
        bool IsAuthorized(IAccount account);

        Task<bool> IsAuthorizedAsync(IAccount account, CancellationToken cancellationToken);
    }
}
