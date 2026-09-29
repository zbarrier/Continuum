using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T}(Result{T}, bool, Func{T, T})"/>
        public static Task<Result<T>> MapIf<T>(this Result<T> result, bool condition, Func<T, Task<T>> func)
        {
            if (!condition)
            {
                return result.AsCompletedTask();
            }

            return result.Map(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T, TContext}(Result{T}, bool, Func{T, TContext, T}, TContext)"/>
        public static Task<Result<T>> MapIf<T, TContext>(
            this Result<T> result,
            bool condition,
            Func<T, TContext, Task<T>> func,
            TContext context
        )
        {
            if (!condition)
            {
                return result.AsCompletedTask();
            }

            return result.Map(func, context);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T}(Result{T}, Func{T, bool}, Func{T, T})"/>
        public static Task<Result<T>> MapIf<T>(this Result<T> result, Func<T, bool> predicate, Func<T, Task<T>> func)
        {
            if (!result.IsSuccess || !predicate(result.Value))
            {
                return result.AsCompletedTask();
            }

            return result.Map(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapIf{T, TContext}(Result{T}, Func{T, TContext, bool}, Func{T, TContext, T}, TContext)"/>
        public static Task<Result<T>> MapIf<T, TContext>(
            this Result<T> result,
            Func<T, TContext, bool> predicate,
            Func<T, TContext, Task<T>> func,
            TContext context
        )
        {
            if (!result.IsSuccess || !predicate(result.Value, context))
            {
                return result.AsCompletedTask();
            }

            return result.Map(func, context);
        }
    }
}
