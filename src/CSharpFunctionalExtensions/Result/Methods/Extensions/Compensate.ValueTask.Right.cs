#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        public static ValueTask<Result> Compensate(this Result result, Func<Error, ValueTask<Result>> valueTask)
        {
            if (result.IsSuccess)
            {
                return Result.Success().AsCompletedValueTask();
            }

            return valueTask(result.Error);
        }

        public static ValueTask<Result> Compensate<T>(this Result<T> result, Func<Error, ValueTask<Result>> valueTask)
        {
            if (result.IsSuccess)
            {
                return Result.Success().AsCompletedValueTask();
            }

            return valueTask(result.Error);
        }

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
#endif