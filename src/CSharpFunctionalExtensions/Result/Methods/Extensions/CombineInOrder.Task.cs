using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Task = System.Threading.Tasks.Task;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
        /// <summary>
        ///     Awaits the tasks sequentially, in order, and combines their results. Returns a success when every result succeeds; otherwise a failure with the combined error.
        /// </summary>
        public static async Task<Result> CombineInOrder(this IEnumerable<Task<Result>> tasks)
        {
            Result[] results = await CompleteInOrder(tasks).DefaultAwait();
            return results.Combine();
        }

        /// <inheritdoc cref="CombineInOrder(IEnumerable{Task{Result}})"/>
        public static async Task<Result<IEnumerable<T>>> CombineInOrder<T>(this IEnumerable<Task<Result<T>>> tasks)
        {
            Result<T>[] results = await CompleteInOrder(tasks).DefaultAwait();
            return results.Combine();
        }

        /// <inheritdoc cref="CombineInOrder(IEnumerable{Task{Result}})"/>
        public static async Task<Result> CombineInOrder(this Task<IEnumerable<Task<Result>>> task)
        {
            IEnumerable<Task<Result>> tasks = await task.DefaultAwait();
            return await tasks.CombineInOrder().DefaultAwait();
        }

        /// <inheritdoc cref="CombineInOrder(IEnumerable{Task{Result}})"/>
        public static async Task<Result<IEnumerable<T>>> CombineInOrder<T>(this Task<IEnumerable<Task<Result<T>>>> task)
        {
            IEnumerable<Task<Result<T>>> tasks = await task.DefaultAwait();
            return await tasks.CombineInOrder().DefaultAwait();
        }

        /// <inheritdoc cref="CombineInOrder(IEnumerable{Task{Result}})"/>
        public static async Task<Result<K>> CombineInOrder<T, K>(this IEnumerable<Task<Result<T>>> tasks, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Result<T>> results = await CompleteInOrder(tasks).DefaultAwait();
            return results.Combine(composer);
        }

        /// <inheritdoc cref="CombineInOrder(IEnumerable{Task{Result}})"/>
        public static async Task<Result<K>> CombineInOrder<T, K>(this Task<IEnumerable<Task<Result<T>>>> task, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Task<Result<T>>> tasks = await task.DefaultAwait();
            return await tasks.CombineInOrder(composer).DefaultAwait();
        }

        /// <summary>
        ///     Awaits each task sequentially, in enumeration order, and returns their results.
        /// </summary>
        public static async Task<T[]> CompleteInOrder<T>(IEnumerable<Task<T>> tasks)
        {
            if (tasks is ICollection<Task<T>> collection)
            {
                var array = new T[collection.Count];
                var index = 0;
                foreach (var task in collection)
                {
                    array[index++] = await task.DefaultAwait();
                }
                return array;
            }

            List<T> results = new List<T>();
            foreach (var task in tasks)
            {
                results.Add(await task.DefaultAwait());
            }
            return results.ToArray();
        }
    }
}
