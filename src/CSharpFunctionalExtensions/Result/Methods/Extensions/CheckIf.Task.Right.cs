using System;
using System.Threading.Tasks;

using Task = System.Threading.Tasks.Task;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsRightOperand
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T}(Result{T}, bool, Func{T, Result})"/>
        public static Task<Result<T>> CheckIf<T>(this Result<T> result, bool condition, Func<T, Task<Result>> func)
        {
            if (condition)
                return result.Check(func);
            else
                return Task.FromResult(result);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T, K}(Result{T}, bool, Func{T, Result{K}})"/>
        public static Task<Result<T>> CheckIf<T, K>(this Result<T> result, bool condition, Func<T, Task<Result<K>>> func)
        {
            if (condition)
                return result.Check(func);
            else
                return Task.FromResult(result);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T}(Result{T}, Func{T, bool}, Func{T, Result})"/>
        public static Task<Result<T>> CheckIf<T>(this Result<T> result, Func<T, bool> predicate, Func<T, Task<Result>> func)
        {
            if (result.IsSuccess && predicate(result.Value))
                return result.Check(func);
            else
                return Task.FromResult(result);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T, K}(Result{T}, Func{T, bool}, Func{T, Result{K}})"/>
        public static Task<Result<T>> CheckIf<T, K>(this Result<T> result, Func<T, bool> predicate, Func<T, Task<Result<K>>> func)
        {
            if (result.IsSuccess && predicate(result.Value))
                return result.Check(func);
            else
                return Task.FromResult(result);
        }
    }
}
