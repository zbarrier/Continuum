using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    internal static class ValueTaskExtensions
    {
        public static ValueTask<T> AsCompletedValueTask<T>(this T obj) => ValueTask.FromResult(obj);

        public static ConfiguredValueTaskAwaitable DefaultAwait(this ValueTask valueTask) =>
            valueTask.ConfigureAwait(Result.Configuration.DefaultConfigureAwait);

        public static ConfiguredValueTaskAwaitable<T> DefaultAwait<T>(this ValueTask<T> valueTask) =>
            valueTask.ConfigureAwait(Result.Configuration.DefaultConfigureAwait);
    }
}