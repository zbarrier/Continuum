using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
        /// <summary>
        ///     Returns the result of the given <paramref name="onSuccessValueTask"/> valueTask action if the calling Result is a success. Otherwise, it returns the result of the given <paramref name="onFailureValueTask"/> valueTask action.
        /// </summary>
        public static ValueTask<K> Match<K, T>(this Result<T> result, Func<T, ValueTask<K>> onSuccessValueTask, Func<Error, ValueTask<K>> onFailureValueTask)
        {
            return result.IsSuccess
                ? onSuccessValueTask(result.Value)
                : onFailureValueTask(result.Error);
        }

        /// <summary>
        ///     Returns the result of the given <paramref name="onSuccessValueTask"/> valueTask action if the calling Result is a success. Otherwise, it returns the result of the given <paramref name="onFailureValueTask"/> valueTask action.
        /// </summary>
        public static ValueTask<T> Match<T>(this Result result, Func<ValueTask<T>> onSuccessValueTask, Func<Error, ValueTask<T>> onFailureValueTask)
        {
            return result.IsSuccess
                ? onSuccessValueTask()
                : onFailureValueTask(result.Error);
        }

        /// <summary>
        ///     Invokes the given <paramref name="onSuccessValueTask"/> action if the calling Result is a success. Otherwise, it invokes the given <paramref name="onFailureValueTask"/> action.
        /// </summary>
        public static ValueTask Match<T>(this Result<T> result, Func<T, ValueTask> onSuccessValueTask, Func<Error, ValueTask> onFailureValueTask)
        {
            return result.IsSuccess
                ? onSuccessValueTask(result.Value)
                : onFailureValueTask(result.Error);
        }

        /// <summary>
        ///     Invokes the given <paramref name="onSuccessValueTask"/> action if the calling Result is a success. Otherwise, it invokes the given <paramref name="onFailureValueTask"/> action.
        /// </summary>
        public static ValueTask Match(this Result result, Func<ValueTask> onSuccessValueTask, Func<Error, ValueTask> onFailureValueTask)
        {
            return result.IsSuccess
                ? onSuccessValueTask()
                : onFailureValueTask(result.Error);
        }
    }
}