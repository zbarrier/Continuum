using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate{T}(Result{T}, Func{Result{T}})"/>
        public static async Task<Result<T>> OnFailureCompensate<T>(this Task<Result<T>> resultTask, Func<Result<T>> func)
        {
            Result<T> result = await resultTask.DefaultAwait();
            return result.OnFailureCompensate(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate(Result, Func{Result})"/>
        public static async Task<Result> OnFailureCompensate(this Task<Result> resultTask, Func<Result> func)
        {
            Result result = await resultTask.DefaultAwait();
            return result.OnFailureCompensate(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate{T}(Result{T}, Func{Error, Result{T}})"/>
        public static async Task<Result<T>> OnFailureCompensate<T>(this Task<Result<T>> resultTask, Func<Error, Result<T>> func)
        {
            Result<T> result = await resultTask.DefaultAwait();
            return result.OnFailureCompensate(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate(Result, Func{Error, Result})"/>
        public static async Task<Result> OnFailureCompensate(this Task<Result> resultTask, Func<Error, Result> func)
        {
            Result result = await resultTask.DefaultAwait();
            return result.OnFailureCompensate(func);
        }
    }
}
