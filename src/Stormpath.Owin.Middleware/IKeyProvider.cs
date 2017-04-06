using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Stormpath.Owin.Middleware
{
    public interface IKeyProvider
    {
        Task<IssuerSigningKeyResolver> GetSigningKeyResolver(CancellationToken cancellationToken);
    }
}