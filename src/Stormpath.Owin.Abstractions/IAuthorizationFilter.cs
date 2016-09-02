using System.Threading;
using System.Threading.Tasks;
using Stormpath.SDK.Account;

namespace Stormpath.Owin.Abstractions
{
    public interface IAuthorizationFilter
    {
        Task<bool> IsAuthorized(IAccount account, CancellationToken cancellationToken);
    }
}
