#if NET5_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
       public static async ValueTask<Result> Combine(this IEnumerable<ValueTask<Result>> tasks)
        {
            Result[] results = await Task.WhenAll(tasks.Select(x=> x.AsTask())).DefaultAwait();
            return Result.Combine(results);
        }

        public static async ValueTask<Result<IEnumerable<T>>> Combine<T>(this IEnumerable<ValueTask<Result<T>>> tasks)
        {
            Result<T>[] results = await Task.WhenAll(tasks.Select(x=> x.AsTask())).DefaultAwait();
            return results.Combine();
        }

        public static async ValueTask<Result> Combine(this ValueTask<IEnumerable<Result>> task)
        {
            IEnumerable<Result> results = await task;
            return Result.Combine(results);
        }

        public static async ValueTask<Result<IEnumerable<T>>> Combine<T>(this ValueTask<IEnumerable<Result<T>>> task)
        {
            IEnumerable<Result<T>> results = await task;
            return results.Combine();
        }

        public static async ValueTask<Result> Combine(this ValueTask<IEnumerable<ValueTask<Result>>> task)
        {
            IEnumerable<ValueTask<Result>> tasks = await task;
            return await tasks.Combine();
        }

        public static async ValueTask<Result<IEnumerable<T>>> Combine<T>(this ValueTask<IEnumerable<ValueTask<Result<T>>>> task)
        {
            IEnumerable<ValueTask<Result<T>>> tasks = await task;
            return await tasks.Combine();
        }

        public static async ValueTask<Result<K>> Combine<T, K>(this IEnumerable<ValueTask<Result<T>>> tasks, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Result<T>> results = await Task.WhenAll(tasks.Select(x=> x.AsTask())).DefaultAwait();
            return results.Combine(composer);
        }

        public static async ValueTask<Result<K>> Combine<T, K>(this ValueTask<IEnumerable<ValueTask<Result<T>>>> task, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<ValueTask<Result<T>>> tasks = await task;
            return await tasks.Combine(composer);
        }
    }
}
#endif