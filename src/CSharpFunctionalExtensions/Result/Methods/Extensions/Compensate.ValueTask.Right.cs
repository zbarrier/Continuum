using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Compensate(Result, Func{Error, Result})"/>
        public static ValueTask<Result> Compensate(this Result result, Func<Error, ValueTask<Result>> valueTask)
        {
            if (result.IsSuccess)
            {
                return Result.Success().AsCompletedValueTask();
            }

            return valueTask(result.Error);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Compensate{T}(Result{T}, Func{Error, Result})"/>
        public static ValueTask<Result> Compensate<T>(this Result<T> result, Func<Error, ValueTask<Result>> valueTask)
        {
            if (result.IsSuccess)
            {
                return Result.Success().AsCompletedValueTask();
            }

            return valueTask(result.Error);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Compensate{T}(Result{T}, Func{Error, Result{T}})"/>
        public static ValueTask<Result<T>> Compensate<T>(this Result<T> result, Func<Error, ValueTask<Result<T>>> valueTask)
        {
            if (result.IsSuccess)
            {
                return Result.Success(result.Value).AsCompletedValueTask();
            }

            return valueTask(result.Error);
        }
    }
}
