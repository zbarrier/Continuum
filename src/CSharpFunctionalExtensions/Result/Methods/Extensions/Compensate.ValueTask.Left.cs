using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Compensate(Result, Func{Error, Result})"/>
        public static async ValueTask<Result> Compensate(this ValueTask<Result> resultTask, Func<Error, Result> valueTask)
        {
            var result = await resultTask;
            return result.Compensate(valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Compensate{T}(Result{T}, Func{Error, Result})"/>
        public static async ValueTask<Result> Compensate<T>(this ValueTask<Result<T>> resultTask, Func<Error, Result> valueTask)
        {
            var result = await resultTask;
            return result.Compensate(valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Compensate{T}(Result{T}, Func{Error, Result{T}})"/>
        public static async ValueTask<Result<T>> Compensate<T>(this ValueTask<Result<T>> resultTask, Func<Error, Result<T>> valueTask)
        {
            var result = await resultTask;
            return result.Compensate(valueTask);
        }
    }
}
