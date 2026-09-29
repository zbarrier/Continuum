using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsBothOperands
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate{T}(Result{T}, Func{Result{T}})"/>
        public static async Task<Result<T>> OnFailureCompensate<T>(this Task<Result<T>> resultTask, Func<Task<Result<T>>> func)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return await func().DefaultAwait();

            return result;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate(Result, Func{Result})"/>
        public static async Task<Result> OnFailureCompensate(this Task<Result> resultTask, Func<Task<Result>> func)
        {
            Result result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return await func().DefaultAwait();

            return result;
        }
        
        
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate{T}(Result{T}, Func{Error, Result{T}})"/>
        public static async Task<Result<T>> OnFailureCompensate<T>(this Task<Result<T>> resultTask, Func<Error, Task<Result<T>>> func)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return await func(result.Error).DefaultAwait();

            return result;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate(Result, Func{Error, Result})"/>
        public static async Task<Result> OnFailureCompensate(this Task<Result> resultTask, Func<Error, Task<Result>> func)
        {
            Result result = await resultTask.DefaultAwait();
            
            if (result.IsFailure)
                return await func(result.Error).DefaultAwait();

            return result;
        }
    }
}
