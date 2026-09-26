#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        public static async ValueTask<Result> Compensate(this ValueTask<Result> resultTask, Func<Error, ValueTask<Result>> valueTask)
        {
            var result = await resultTask;
            return await result.Compensate(valueTask);
        }

        public static async ValueTask<Result> Compensate<T>(this ValueTask<Result<T>> resultTask, Func<Error, ValueTask<Result>> valueTask)
        {
            var result = await resultTask;
            return await result.Compensate(valueTask);
        }

        public static async ValueTask<Result<T>> Compensate<T>(this ValueTask<Result<T>> resultTask, Func<Error, ValueTask<Result<T>>> valueTask)
        {
            var result = await resultTask;
            return await result.Compensate(valueTask);
        }
    }
}
#endif