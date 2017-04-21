using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Abstractions
{
    public interface IAuthorizationFilter
    {
        bool IsAuthorized(ICompatibleOktaAccount account);

        [Obsolete("Use the synchronous IsAuthorized")]
        Task<bool> IsAuthorizedAsync(ICompatibleOktaAccount account, CancellationToken cancellationToken);
    }
}
