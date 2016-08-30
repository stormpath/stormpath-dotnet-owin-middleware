using System.Threading.Tasks;

namespace Stormpath.Owin.Abstractions
{
    public static class TaskConstants
    {
        public static Task<bool> CompletedTask = Task.FromResult(true);
    }
}
