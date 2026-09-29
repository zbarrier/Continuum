using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
       /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine(IEnumerable{Result})"/>
       public static async ValueTask<Result> Combine(this IEnumerable<ValueTask<Result>> tasks)
        {
            Result[] results = await Task.WhenAll(tasks.Select(x=> x.AsTask())).DefaultAwait();
            return Result.Combine(results);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine{T}(IEnumerable{Result{T}})"/>
        public static async ValueTask<Result<IEnumerable<T>>> Combine<T>(this IEnumerable<ValueTask<Result<T>>> tasks)
        {
            Result<T>[] results = await Task.WhenAll(tasks.Select(x=> x.AsTask())).DefaultAwait();
            return results.Combine();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine(IEnumerable{Result})"/>
        public static async ValueTask<Result> Combine(this ValueTask<IEnumerable<Result>> task)
        {
            IEnumerable<Result> results = await task;
            return Result.Combine(results);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine{T}(IEnumerable{Result{T}})"/>
        public static async ValueTask<Result<IEnumerable<T>>> Combine<T>(this ValueTask<IEnumerable<Result<T>>> task)
        {
            IEnumerable<Result<T>> results = await task;
            return results.Combine();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine(IEnumerable{Result})"/>
        public static async ValueTask<Result> Combine(this ValueTask<IEnumerable<ValueTask<Result>>> task)
        {
            IEnumerable<ValueTask<Result>> tasks = await task;
            return await tasks.Combine();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine{T}(IEnumerable{Result{T}})"/>
        public static async ValueTask<Result<IEnumerable<T>>> Combine<T>(this ValueTask<IEnumerable<ValueTask<Result<T>>>> task)
        {
            IEnumerable<ValueTask<Result<T>>> tasks = await task;
            return await tasks.Combine();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine{T, K}(IEnumerable{Result{T}}, Func{IEnumerable{T}, K})"/>
        public static async ValueTask<Result<K>> Combine<T, K>(this IEnumerable<ValueTask<Result<T>>> tasks, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<Result<T>> results = await Task.WhenAll(tasks.Select(x=> x.AsTask())).DefaultAwait();
            return results.Combine(composer);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.Combine{T, K}(IEnumerable{Result{T}}, Func{IEnumerable{T}, K})"/>
        public static async ValueTask<Result<K>> Combine<T, K>(this ValueTask<IEnumerable<ValueTask<Result<T>>>> task, Func<IEnumerable<T>, K> composer)
        {
            IEnumerable<ValueTask<Result<T>>> tasks = await task;
            return await tasks.Combine(composer);
        }
    }
}
