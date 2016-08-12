using System.Threading.Tasks;

namespace Stormpath.Owin.Abstractions
{
    public static class TaskConstants
    {
        public static Task CompletedTask = Task.FromResult(true);
    }
}
