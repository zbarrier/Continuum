using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Compensate(Result, Func{Error, Result})"/>
        public static Task<Result> Compensate(this Result result, Func<Error, Task<Result>> func)
        {
            if (result.IsSuccess)
            {
                return Result.Success().AsCompletedTask();
            }

            return func(result.Error);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Compensate{T}(Result{T}, Func{Error, Result})"/>
        public static Task<Result> Compensate<T>(this Result<T> result, Func<Error, Task<Result>> func)
        {
            if (result.IsSuccess)
            {
                return Result.Success().AsCompletedTask();
            }

            return func(result.Error);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Compensate{T}(Result{T}, Func{Error, Result{T}})"/>
        public static Task<Result<T>> Compensate<T>(this Result<T> result, Func<Error, Task<Result<T>>> func)
        {
            if (result.IsSuccess)
            {
                return Result.Success(result.Value).AsCompletedTask();
            }

            return func(result.Error);
        }
    }
}
