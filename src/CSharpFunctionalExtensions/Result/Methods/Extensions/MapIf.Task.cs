using System;
using System.Threading.Tasks;

using Continuum.CSharpFunctionalExtensions.ValueTasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T}(Result{T}, bool, Func{T, T})"/>
        public static async Task<Result<T>> MapIf<T>(this Task<Result<T>> resultTask, bool condition, Func<T, Task<T>> func)
        {
            var result = await resultTask.DefaultAwait();
            return await result.MapIf(condition, func).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T, TContext}(Result{T}, bool, Func{T, TContext, T}, TContext)"/>
        public static async Task<Result<T>> MapIf<T, TContext>(
            this Task<Result<T>> resultTask,
            bool condition,
            Func<T, TContext, Task<T>> func,
            TContext context
        )
        {
            var result = await resultTask.DefaultAwait();
            return await result.MapIf(condition, func, context).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T}(Result{T}, Func{T, bool}, Func{T, T})"/>
        public static async Task<Result<T>> MapIf<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Func<T, Task<T>> func)
        {
            var result = await resultTask.DefaultAwait();
            return await result.MapIf(predicate, func).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T, TContext}(Result{T}, Func{T, TContext, bool}, Func{T, TContext, T}, TContext)"/>
        public static async Task<Result<T>> MapIf<T, TContext>(
            this Task<Result<T>> resultTask,
            Func<T, TContext, bool> predicate,
            Func<T, TContext, Task<T>> func,
            TContext context
        )
        {
            var result = await resultTask.DefaultAwait();
            return await result.MapIf(predicate, func, context).DefaultAwait();
        }
    }
}
