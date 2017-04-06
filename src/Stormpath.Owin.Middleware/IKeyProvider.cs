using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Stormpath.Owin.Middleware
{
    interface IKeyProvider
    {
        Task<IssuerSigningKeyResolver> GetSigningKeyResolver(CancellationToken cancellationToken);
    }
}