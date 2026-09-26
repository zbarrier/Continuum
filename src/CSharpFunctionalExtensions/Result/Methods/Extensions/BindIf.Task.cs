using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        public static async Task<Result> BindIf(this Task<Result> resultTask, bool condition, Func<Task<Result>> func)
        {
            var result = await resultTask.DefaultAwait();
            return await result.BindIf(condition, func).DefaultAwait();
        }

        public static async Task<Result<T>> BindIf<T>(this Task<Result<T>> resultTask, bool condition, Func<T, Task<Result<T>>> func)
        {
            var result = await resultTask.DefaultAwait();
            return await result.BindIf(condition, func).DefaultAwait();
        }

        public static async Task<Result> BindIf(this Task<Result> resultTask, Func<bool> predicate, Func<Task<Result>> func)
        {
            var result = await resultTask.DefaultAwait();
            return await result.BindIf(predicate, func).DefaultAwait();
        }

        public static async Task<Result<T>> BindIf<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Func<T, Task<Result<T>>> func)
        {
            var result = await resultTask.DefaultAwait();
            return await result.BindIf(predicate, func).DefaultAwait();
        }
    }
}