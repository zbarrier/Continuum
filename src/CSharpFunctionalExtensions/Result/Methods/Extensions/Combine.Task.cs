using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Task = System.Threading.Tasks.Task;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine(IEnumerable{Result})"/>
        public static async Task<Result> Combine(this IEnumerable<Task<Result>> tasks)
        {
            Result[] results = await Task.WhenAll(tasks).DefaultAwait();
            return results.Combine();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine{T}(IEnumerable{Result{T}})"/>
        public static async Task<Result<IEnumerable<T>>> Combine<T>(this IEnumerable<Task<Result<T>>> tasks)
        {
            Result<T>[] results = await Task.WhenAll(tasks).DefaultAwait();
            return results.Combine();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine(IEnumerable{Result})"/>
        public static async Task<Result> Combine(this Task<IEnumerable<Result>> task)
        {
            IEnumerable<Result> results = await task.DefaultAwait();
            return results.Combine();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine{T}(IEnumerable{Result{T}})"/>
        public static async Task<Result<IEnumerable<T>>> Combine<T>(this Task<IEnumerable<Result<T>>> task)
        {
            IEnumerable<Result<T>> results = await task.DefaultAwait();
            return results.Combine();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine(IEnumerable{Result})"/>
        public static async Task<Result> Combine(this Task<IEnumerable<Task<Result>>> task)
        {
            IEnumerable<Task<Result>> tasks = await task.DefaultAwait();
            return await tasks.Combine().DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine{T}(IEnumerable{Result{T}})"/>
        public static async Task<Result<IEnumerable<T>>> Combine<T>(this Task<IEnumerable<Task<Result<T>>>> task)
        {
            IEnumerable<Task<Result<T>>> tasks = await task.DefaultAwait();
            return await tasks.Combine().DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine{T, K}(IEnumerable{Result{T}}, Func{IEnumerable{T}, K})"/>
        public static async Task<Result<K>> Combine<T, K>(this IEnumerable<Task<Result<T>>> tasks, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Result<T>> results = await Task.WhenAll(tasks).DefaultAwait();
            return results.Combine(composer);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine{T, K}(IEnumerable{Result{T}}, Func{IEnumerable{T}, K})"/>
        public static async Task<Result<K>> Combine<T, K>(this Task<IEnumerable<Task<Result<T>>>> task, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Task<Result<T>>> tasks = await task.DefaultAwait();
            return await tasks.Combine(composer).DefaultAwait();
        }
    }
}
