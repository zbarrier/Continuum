using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf(Result, bool, Func{Result})"/>
        public static Task<Result> BindIf(this Result result, bool condition, Func<Task<Result>> func)
        {
            if (!condition)
            {
                return result.AsCompletedTask();
            }

            return result.Bind(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf{T}(Result{T}, bool, Func{T, Result{T}})"/>
        public static Task<Result<T>> BindIf<T>(this Result<T> result, bool condition, Func<T, Task<Result<T>>> func)
        {
            if (!condition)
            {
                return result.AsCompletedTask();
            }

            return result.Bind(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf(Result, Func{bool}, Func{Result})"/>
        public static Task<Result> BindIf(this Result result, Func<bool> predicate, Func<Task<Result>> func)
        {
            if (!result.IsSuccess || !predicate())
            {
                return result.AsCompletedTask();
            }

            return result.Bind(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf{T}(Result{T}, Func{T, bool}, Func{T, Result{T}})"/>
        public static Task<Result<T>> BindIf<T>(this Result<T> result, Func<T, bool> predicate, Func<T, Task<Result<T>>> func)
        {
            if (!result.IsSuccess || !predicate(result.Value))
            {
                return result.AsCompletedTask();
            }

            return result.Bind(func);
        }
    }
}
