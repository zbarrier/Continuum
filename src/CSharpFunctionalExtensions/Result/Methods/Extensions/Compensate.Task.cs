using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        public static async Task<Result> Compensate(this Task<Result> resultTask, Func<Error, Task<Result>> func)
        {
            var result = await resultTask.DefaultAwait();
            return await result.Compensate(func).DefaultAwait();
        }

        public static async Task<Result> Compensate<T>(this Task<Result<T>> resultTask, Func<Error, Task<Result>> func)
        {
            var result = await resultTask.DefaultAwait();
            return await result.Compensate(func).DefaultAwait();
        }

        public static async Task<Result<T>> Compensate<T>(this Task<Result<T>> resultTask, Func<Error, Task<Result<T>>> func)
        {
            var result = await resultTask.DefaultAwait();
            return await result.Compensate(func).DefaultAwait();
        }
    }
}