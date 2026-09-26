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
        public static async Task<Result> CombineInOrder(this IEnumerable<Task<Result>> tasks)
        {
            Result[] results = await CompleteInOrder(tasks).DefaultAwait();
            return results.Combine();
        }

        public static async Task<Result<IEnumerable<T>>> CombineInOrder<T>(this IEnumerable<Task<Result<T>>> tasks)
        {
            Result<T>[] results = await CompleteInOrder(tasks).DefaultAwait();
            return results.Combine();
        }

        public static async Task<Result> CombineInOrder(this Task<IEnumerable<Task<Result>>> task)
        {
            IEnumerable<Task<Result>> tasks = await task.DefaultAwait();
            return await tasks.CombineInOrder().DefaultAwait();
        }

        public static async Task<Result<IEnumerable<T>>> CombineInOrder<T>(this Task<IEnumerable<Task<Result<T>>>> task)
        {
            IEnumerable<Task<Result<T>>> tasks = await task.DefaultAwait();
            return await tasks.CombineInOrder().DefaultAwait();
        }

        public static async Task<Result<K>> CombineInOrder<T, K>(this IEnumerable<Task<Result<T>>> tasks, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Result<T>> results = await CompleteInOrder(tasks).DefaultAwait();
            return results.Combine(composer);
        }

        public static async Task<Result<K>> CombineInOrder<T, K>(this Task<IEnumerable<Task<Result<T>>>> task, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Task<Result<T>>> tasks = await task.DefaultAwait();
            return await tasks.CombineInOrder(composer).DefaultAwait();
        }

        public static async Task<T[]> CompleteInOrder<T>(IEnumerable<Task<T>> tasks)
        {
            List<T> results = new List<T>();
            foreach (var task in tasks)
            {
                results.Add(await task.DefaultAwait());
            }
            return results.ToArray();
        }
    }
}
