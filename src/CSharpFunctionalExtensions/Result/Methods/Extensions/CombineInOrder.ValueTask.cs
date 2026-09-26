#if NET5_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
        public static async ValueTask<Result> CombineInOrder(this IEnumerable<ValueTask<Result>> tasks)
        {
            Result[] results = await CompleteInOrder(tasks);
            return results.Combine();
        }

        public static async ValueTask<Result<IEnumerable<T>>> CombineInOrder<T>(this IEnumerable<ValueTask<Result<T>>> tasks)
        {
            Result<T>[] results = await CompleteInOrder(tasks);
            return results.Combine();
        }

        public static async ValueTask<Result> CombineInOrder(this ValueTask<IEnumerable<ValueTask<Result>>> task)
        {
            IEnumerable<ValueTask<Result>> tasks = await task;
            return await tasks.CombineInOrder();
        }

        public static async ValueTask<Result<IEnumerable<T>>> CombineInOrder<T>(this ValueTask<IEnumerable<ValueTask<Result<T>>>> task)
        {
            IEnumerable<ValueTask<Result<T>>> tasks = await task;
            return await tasks.CombineInOrder();
        }

        public static async ValueTask<Result<K>> CombineInOrder<T, K>(this IEnumerable<ValueTask<Result<T>>> tasks, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Result<T>> results = await CompleteInOrder(tasks);
            return results.Combine(composer);
        }

        public static async ValueTask<Result<K>> CombineInOrder<T, K>(this ValueTask<IEnumerable<ValueTask<Result<T>>>> task, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<ValueTask<Result<T>>> tasks = await task;
            return await tasks.CombineInOrder(composer);
        }

        public static async ValueTask<T[]> CompleteInOrder<T>(IEnumerable<ValueTask<T>> tasks)
        {
            List<T> results = new List<T>();
            foreach (var task in tasks)
            {
                results.Add(await task);
            }
            return results.ToArray();
        }
    }
}
#endif
