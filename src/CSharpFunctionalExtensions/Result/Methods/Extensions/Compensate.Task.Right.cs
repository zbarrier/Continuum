using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        public static Task<Result> Compensate(this Result result, Func<Error, Task<Result>> func)
        {
            if (result.IsSuccess)
            {
                return Result.Success().AsCompletedTask();
            }

            return func(result.Error);
        }

        public static Task<Result> Compensate<T>(this Result<T> result, Func<Error, Task<Result>> func)
        {
            if (result.IsSuccess)
            {
                return Result.Success().AsCompletedTask();
            }

            return func(result.Error);
        }

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