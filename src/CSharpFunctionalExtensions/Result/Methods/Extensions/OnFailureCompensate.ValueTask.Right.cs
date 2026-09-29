using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsRightOperand
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate{T}(Result{T}, Func{Result{T}})"/>
        public static async ValueTask<Result<T>> OnFailureCompensate<T>(this Result<T> result, Func<ValueTask<Result<T>>> valueTask)
        {
            if (result.IsFailure)
                return await valueTask();

            return result;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate(Result, Func{Result})"/>
        public static async ValueTask<Result> OnFailureCompensate(this Result result, Func<ValueTask<Result>> valueTask)
        {
            if (result.IsFailure)
                return await valueTask();

            return result;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate{T}(Result{T}, Func{Error, Result{T}})"/>
        public static async ValueTask<Result<T>> OnFailureCompensate<T>(this Result<T> result, Func<Error, ValueTask<Result<T>>> valueTask)
        {
            if (result.IsFailure)
                return await valueTask(result.Error);

            return result;
        }
        
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate(Result, Func{Error, Result})"/>
        public static async ValueTask<Result> OnFailureCompensate(this Result result, Func<Error, ValueTask<Result>> valueTask)
        {
            if (result.IsFailure)
                return await valueTask(result.Error);

            return result;
        }
    }
}
