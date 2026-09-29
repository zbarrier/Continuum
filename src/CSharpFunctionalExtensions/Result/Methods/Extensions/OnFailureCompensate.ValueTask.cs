using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsBothOperands
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate{T}(Result{T}, Func{Result{T}})"/>
        public static async ValueTask<Result<T>> OnFailureCompensate<T>(this ValueTask<Result<T>> resultTask, Func<ValueTask<Result<T>>> valueTask)
        {
            Result<T> result = await resultTask;

            if (result.IsFailure)
                return await valueTask();

            return result;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate(Result, Func{Result})"/>
        public static async ValueTask<Result> OnFailureCompensate(this ValueTask<Result> resultTask, Func<ValueTask<Result>> valueTask)
        {
            Result result = await resultTask;

            if (result.IsFailure)
                return await valueTask();

            return result;
        }
        
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate{T}(Result{T}, Func{Error, Result{T}})"/>
        public static async ValueTask<Result<T>> OnFailureCompensate<T>(this ValueTask<Result<T>> resultTask, Func<Error, ValueTask<Result<T>>> valueTask)
        {
            Result<T> result = await resultTask;

            if (result.IsFailure)
                return await valueTask(result.Error);

            return result;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate(Result, Func{Error, Result})"/>
        public static async ValueTask<Result> OnFailureCompensate(this ValueTask<Result> resultTask, Func<Error, ValueTask<Result>> valueTask)
        {
            Result result = await resultTask;
            
            if (result.IsFailure)
                return await valueTask(result.Error);

            return result;
        }
    }
}
