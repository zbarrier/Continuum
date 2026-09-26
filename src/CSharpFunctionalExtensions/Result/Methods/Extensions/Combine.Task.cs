using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#if NET40
using Task = System.Threading.Tasks.TaskEx;
#else
using Task = System.Threading.Tasks.Task;
#endif

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
        public static async Task<Result> Combine(this IEnumerable<Task<Result>> tasks)
        {
            Result[] results = await Task.WhenAll(tasks).DefaultAwait();
            return results.Combine();
        }

        public static async Task<Result<IEnumerable<T>>> Combine<T>(this IEnumerable<Task<Result<T>>> tasks)
        {
            Result<T>[] results = await Task.WhenAll(tasks).DefaultAwait();
            return results.Combine();
        }

        public static async Task<Result> Combine(this Task<IEnumerable<Result>> task)
        {
            IEnumerable<Result> results = await task.DefaultAwait();
            return results.Combine();
        }

        public static async Task<Result<IEnumerable<T>>> Combine<T>(this Task<IEnumerable<Result<T>>> task)
        {
            IEnumerable<Result<T>> results = await task.DefaultAwait();
            return results.Combine();
        }

        public static async Task<Result> Combine(this Task<IEnumerable<Task<Result>>> task)
        {
            IEnumerable<Task<Result>> tasks = await task.DefaultAwait();
            return await tasks.Combine().DefaultAwait();
        }

        public static async Task<Result<IEnumerable<T>>> Combine<T>(this Task<IEnumerable<Task<Result<T>>>> task)
        {
            IEnumerable<Task<Result<T>>> tasks = await task.DefaultAwait();
            return await tasks.Combine().DefaultAwait();
        }

        public static async Task<Result<K>> Combine<T, K>(this IEnumerable<Task<Result<T>>> tasks, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Result<T>> results = await Task.WhenAll(tasks).DefaultAwait();
            return results.Combine(composer);
        }

        public static async Task<Result<K>> Combine<T, K>(this Task<IEnumerable<Task<Result<T>>>> task, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Task<Result<T>>> tasks = await task.DefaultAwait();
            return await tasks.Combine(composer).DefaultAwait();
        }
    }
}
