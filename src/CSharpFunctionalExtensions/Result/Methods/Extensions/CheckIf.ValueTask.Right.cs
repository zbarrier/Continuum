using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsRightOperand
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T}(Result{T}, bool, Func{T, Result})"/>
        public static ValueTask<Result<T>> CheckIf<T>(this Result<T> result, bool condition, Func<T, ValueTask<Result>> valueTask)
        {
            if (condition)
                return result.Check(valueTask);
            else
                return result.AsCompletedValueTask();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T, K}(Result{T}, bool, Func{T, Result{K}})"/>
        public static ValueTask<Result<T>> CheckIf<T, K>(this Result<T> result, bool condition, Func<T, ValueTask<Result<K>>> valueTask)
        {
            if (condition)
                return result.Check(valueTask);
            else
                return result.AsCompletedValueTask();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T}(Result{T}, Func{T, bool}, Func{T, Result})"/>
        public static ValueTask<Result<T>> CheckIf<T>(this Result<T> result, Func<T, bool> predicate, Func<T, ValueTask<Result>> valueTask)
        {
            if (result.IsSuccess && predicate(result.Value))
                return result.Check(valueTask);
            else
                return result.AsCompletedValueTask();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T, K}(Result{T}, Func{T, bool}, Func{T, Result{K}})"/>
        public static ValueTask<Result<T>> CheckIf<T, K>(this Result<T> result, Func<T, bool> predicate, Func<T, ValueTask<Result<K>>> valueTask)
        {
            if (result.IsSuccess && predicate(result.Value))
                return result.Check(valueTask);
            else
                return result.AsCompletedValueTask();
        }
    }
}
