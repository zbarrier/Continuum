using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
        /// <summary>
        ///     Awaits the tasks sequentially, in order, and combines their results. Returns a success when every result succeeds; otherwise a failure with the combined error.
        /// </summary>
        public static async ValueTask<Result> CombineInOrder(this IEnumerable<ValueTask<Result>> tasks)
        {
            Result[] results = await CompleteInOrder(tasks);
            return results.Combine();
        }

        /// <inheritdoc cref="CombineInOrder(IEnumerable{ValueTask{Result}})"/>
        public static async ValueTask<Result<IEnumerable<T>>> CombineInOrder<T>(this IEnumerable<ValueTask<Result<T>>> tasks)
        {
            Result<T>[] results = await CompleteInOrder(tasks);
            return results.Combine();
        }

        /// <inheritdoc cref="CombineInOrder(IEnumerable{ValueTask{Result}})"/>
        public static async ValueTask<Result> CombineInOrder(this ValueTask<IEnumerable<ValueTask<Result>>> task)
        {
            IEnumerable<ValueTask<Result>> tasks = await task;
            return await tasks.CombineInOrder();
        }

        /// <inheritdoc cref="CombineInOrder(IEnumerable{ValueTask{Result}})"/>
        public static async ValueTask<Result<IEnumerable<T>>> CombineInOrder<T>(this ValueTask<IEnumerable<ValueTask<Result<T>>>> task)
        {
            IEnumerable<ValueTask<Result<T>>> tasks = await task;
            return await tasks.CombineInOrder();
        }

        /// <inheritdoc cref="CombineInOrder(IEnumerable{ValueTask{Result}})"/>
        public static async ValueTask<Result<K>> CombineInOrder<T, K>(this IEnumerable<ValueTask<Result<T>>> tasks, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Result<T>> results = await CompleteInOrder(tasks);
            return results.Combine(composer);
        }

        /// <inheritdoc cref="CombineInOrder(IEnumerable{ValueTask{Result}})"/>
        public static async ValueTask<Result<K>> CombineInOrder<T, K>(this ValueTask<IEnumerable<ValueTask<Result<T>>>> task, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<ValueTask<Result<T>>> tasks = await task;
            return await tasks.CombineInOrder(composer);
        }

        /// <summary>
        ///     Awaits each task sequentially, in enumeration order, and returns their results.
        /// </summary>
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
