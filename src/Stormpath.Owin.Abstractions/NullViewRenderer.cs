using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.Owin.Abstractions
{
    public sealed class NullViewRenderer : IViewRenderer
    {
        public Task<bool> RenderAsync(string name, object model, IOwinEnvironment context, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }
    }
}
