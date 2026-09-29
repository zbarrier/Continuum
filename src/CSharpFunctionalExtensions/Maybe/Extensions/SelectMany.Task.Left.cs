using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     This method should be used in linq queries. We recommend using Bind method.
        /// </summary>
        public static Task<Maybe<K>> SelectMany<T, K>(this Task<Maybe<T>> maybeTask, Func<T, Maybe<K>> selector)
        {
            return maybeTask.Bind(selector);
        }

        /// <summary>
        ///     This method should be used in linq queries. We recommend using Bind method.
        /// </summary>
        public static async Task<Maybe<V>> SelectMany<T, U, V>(
            this Task<Maybe<T>> maybeTask,
            Func<T, Maybe<U>> selector,
            Func<T, U, V> project)
        {
            var maybe = await maybeTask.DefaultAwait();
            return maybe.SelectMany(selector, project);
        }
    }
}
