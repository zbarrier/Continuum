using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     This method should be used in linq queries. We recommend using Bind method.
        /// </summary>
        public static ValueTask<Maybe<K>> SelectMany<T, K>(this Maybe<T> maybe, Func<T, ValueTask<Maybe<K>>> selector)
        {
            return maybe.Bind(selector);
        }

        /// <summary>
        ///     This method should be used in linq queries. We recommend using Bind method.
        /// </summary>
        public static async ValueTask<Maybe<V>> SelectMany<T, U, V>(
            this Maybe<T> maybe,
            Func<T, ValueTask<Maybe<U>>> selector,
            Func<T, U, V> project)
        {
            if (maybe.HasNoValue)
                return Maybe<V>.None;

            var value = maybe.GetValueOrThrow();
            var inner = await selector(value);
            return inner.Map(x => project(value, x));
        }
    }
}
