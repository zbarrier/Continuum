using System.Threading.Tasks;

using Task = System.Threading.Tasks.Task;
using System.Runtime.CompilerServices;

namespace Continuum.CSharpFunctionalExtensions
{
    internal static class TaskExtensions
    {
        public static Task<T> AsCompletedTask<T>(this T obj) => Task.FromResult(obj);

        public static ConfiguredTaskAwaitable DefaultAwait(this System.Threading.Tasks.Task task) =>
            task.ConfigureAwait(Result.Configuration.DefaultConfigureAwait);

        public static ConfiguredTaskAwaitable<T> DefaultAwait<T>(this Task<T> task) =>
            task.ConfigureAwait(Result.Configuration.DefaultConfigureAwait);
    }
}
