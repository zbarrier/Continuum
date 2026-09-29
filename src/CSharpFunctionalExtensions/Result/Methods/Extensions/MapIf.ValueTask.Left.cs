using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T}(Result{T}, bool, Func{T, T})"/>
        public static async ValueTask<Result<T>> MapIf<T>(this ValueTask<Result<T>> resultTask, bool condition, Func<T, T> valueTask)
        {
            var result = await resultTask;
            return result.MapIf(condition, valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T, TContext}(Result{T}, bool, Func{T, TContext, T}, TContext)"/>
        public static async ValueTask<Result<T>> MapIf<T, TContext>(
            this ValueTask<Result<T>> resultTask,
            bool condition,
            Func<T, TContext, T> valueTask,
            TContext context
        )
        {
            var result = await resultTask;
            return result.MapIf(condition, valueTask, context);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T}(Result{T}, Func{T, bool}, Func{T, T})"/>
        public static async ValueTask<Result<T>> MapIf<T>(this ValueTask<Result<T>> resultTask, Func<T, bool> predicate, Func<T, T> valueTask)
        {
            var result = await resultTask;
            return result.MapIf(predicate, valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T, TContext}(Result{T}, Func{T, TContext, bool}, Func{T, TContext, T}, TContext)"/>
        public static async ValueTask<Result<T>> MapIf<T, TContext>(
            this ValueTask<Result<T>> resultTask,
            Func<T, TContext, bool> predicate,
            Func<T, TContext, T> valueTask,
            TContext context
        )
        {
            var result = await resultTask;
            return result.MapIf(predicate, valueTask, context);
        }
    }
}
