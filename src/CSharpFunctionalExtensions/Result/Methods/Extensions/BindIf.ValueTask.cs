using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    /// <summary>
    ///     Extension methods for composing <see cref="Result"/> and <see cref="Result{T}"/> with <see cref="System.Threading.Tasks.ValueTask{TResult}"/>-based delegates.
    /// </summary>
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf(Result, bool, Func{Result})"/>
        public static async ValueTask<Result> BindIf(this ValueTask<Result> resultTask, bool condition, Func<ValueTask<Result>> valueTask)
        {
            var result = await resultTask;
            return await result.BindIf(condition, valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf{T}(Result{T}, bool, Func{T, Result{T}})"/>
        public static async ValueTask<Result<T>> BindIf<T>(this ValueTask<Result<T>> resultTask, bool condition, Func<T, ValueTask<Result<T>>> valueTask)
        {
            var result = await resultTask;
            return await result.BindIf(condition, valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf(Result, Func{bool}, Func{Result})"/>
        public static async ValueTask<Result> BindIf(this ValueTask<Result> resultTask, Func<bool> predicate, Func<ValueTask<Result>> valueTask)
        {
            var result = await resultTask;
            return await result.BindIf(predicate, valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf{T}(Result{T}, Func{T, bool}, Func{T, Result{T}})"/>
        public static async ValueTask<Result<T>> BindIf<T>(this ValueTask<Result<T>> resultTask, Func<T, bool> predicate, Func<T, ValueTask<Result<T>>> valueTask)
        {
            var result = await resultTask;
            return await result.BindIf(predicate, valueTask);
        }
    }
}
