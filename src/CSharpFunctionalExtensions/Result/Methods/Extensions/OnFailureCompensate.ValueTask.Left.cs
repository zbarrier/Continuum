using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsLeftOperand
    {        
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate{T}(Result{T}, Func{Result{T}})"/>
        public static async ValueTask<Result<T>> OnFailureCompensate<T>(this ValueTask<Result<T>> resultTask, Func<Result<T>> valueTask)
        {
            Result<T> result = await resultTask;
            return result.OnFailureCompensate(valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate(Result, Func{Result})"/>
        public static async ValueTask<Result> OnFailureCompensate(this ValueTask<Result> resultTask, Func<Result> valueTask)
        {
            Result result = await resultTask;
            return result.OnFailureCompensate(valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate{T}(Result{T}, Func{Error, Result{T}})"/>
        public static async ValueTask<Result<T>> OnFailureCompensate<T>(this ValueTask<Result<T>> resultTask, Func<Error, Result<T>> valueTask)
        {
            Result<T> result = await resultTask;
            return result.OnFailureCompensate(valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnFailureCompensate(Result, Func{Error, Result})"/>
        public static async ValueTask<Result> OnFailureCompensate(this ValueTask<Result> resultTask, Func<Error, Result> valueTask)
        {
            Result result = await resultTask;
            return result.OnFailureCompensate(valueTask);
        }
    }
}
