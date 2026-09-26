#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
        /// <summary>
        ///     Returns the result of the given <paramref name="onSuccess"/> valueTask action if the calling Result is a success. Otherwise, it returns the result of the given <paramref name="onFailure"/> valueTask action.
        /// </summary>
        public static async ValueTask<K> Match<K, T>(this ValueTask<Result<T>> resultTask, Func<T, K> onSuccess, Func<Error, K> onFailure)
        {
            return (await resultTask).Match(onSuccess, onFailure);
        }

        /// <summary>
        ///     Returns the result of the given <paramref name="onSuccess"/> valueTask action if the calling Result is a success. Otherwise, it returns the result of the given <paramref name="onFailure"/> valueTask action.
        /// </summary>
        public static async ValueTask<T> Match<T>(this ValueTask<Result> resultTask, Func<T> onSuccess, Func<Error, T> onFailure)
        {
            return (await resultTask).Match(onSuccess, onFailure);
        }

        /// <summary>
        ///     Invokes the given <paramref name="onSuccess"/> action if the calling Result is a success. Otherwise, it invokes the given <paramref name="onFailure"/> action.
        /// </summary>
        public static async Task Match<T>(this ValueTask<Result<T>> resultTask, Action<T> onSuccess, Action<Error> onFailure)
        {
            (await resultTask).Match(onSuccess, onFailure);
        }

        /// <summary>
        ///     Invokes the given <paramref name="onSuccess"/> action if the calling Result is a success. Otherwise, it invokes the given <paramref name="onFailure"/> action.
        /// </summary>
        public static async Task Match(this ValueTask<Result> resultTask, Action onSuccess, Action<Error> onFailure)
        {
            (await resultTask).Match(onSuccess, onFailure);
        }
    }
}
#endif