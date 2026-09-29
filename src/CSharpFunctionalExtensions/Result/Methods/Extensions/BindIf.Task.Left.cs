using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf(Result, bool, Func{Result})"/>
        public static async Task<Result> BindIf(this Task<Result> resultTask, bool condition, Func<Result> func)
        {
            var result = await resultTask.DefaultAwait();
            return result.BindIf(condition, func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf{T}(Result{T}, bool, Func{T, Result{T}})"/>
        public static async Task<Result<T>> BindIf<T>(this Task<Result<T>> resultTask, bool condition, Func<T, Result<T>> func)
        {
            var result = await resultTask.DefaultAwait();
            return result.BindIf(condition, func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf(Result, Func{bool}, Func{Result})"/>
        public static async Task<Result> BindIf(this Task<Result> resultTask, Func<bool> predicate, Func<Result> func)
        {
            var result = await resultTask.DefaultAwait();
            return result.BindIf(predicate, func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf{T}(Result{T}, Func{T, bool}, Func{T, Result{T}})"/>
        public static async Task<Result<T>> BindIf<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Func<T, Result<T>> func)
        {
            var result = await resultTask.DefaultAwait();
            return result.BindIf(predicate, func);
        }
    }
}
