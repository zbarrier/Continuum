using System;
using System.Collections.Generic;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Returns the result of <paramref name="Some"/> when a value is present, otherwise the result of <paramref name="None"/>.
        /// </summary>
        /// <typeparam name="TE">The type of the returned value.</typeparam>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="Some">Invoked with the inner value.</param>
        /// <param name="None">Invoked when empty.</param>
        /// <returns>The result of whichever delegate was invoked.</returns>
        public static TE Match<TE, T>(in this Maybe<T> maybe, Func<T, TE> Some, Func<TE> None)
        {
            return maybe.HasValue ? Some(maybe.GetValueOrThrow()) : None();
        }

        /// <inheritdoc cref="Match{TE, T}(in Maybe{T}, Func{T, TE}, Func{TE})"/>
        public static TE Match<TE, T, TContext>(
            in this Maybe<T> maybe,
            Func<T, TContext, TE> Some,
            Func<TContext, TE> None,
            TContext context
        )
        {
            return maybe.HasValue ? Some(maybe.GetValueOrThrow(), context) : None(context);
        }

        /// <summary>
        ///     Invokes <paramref name="Some"/> when a value is present, otherwise <paramref name="None"/>.
        /// </summary>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="Some">Invoked with the inner value.</param>
        /// <param name="None">Invoked when empty.</param>
        public static void Match<T>(in this Maybe<T> maybe, Action<T> Some, Action None)
        {
            if (maybe.HasValue)
                Some(maybe.GetValueOrThrow());
            else
                None();
        }

        /// <inheritdoc cref="Match{T}(in Maybe{T}, Action{T}, Action)"/>
        public static void Match<T, TContext>(
            in this Maybe<T> maybe,
            Action<T, TContext> Some,
            Action<TContext> None,
            TContext context
        )
        {
            if (maybe.HasValue)
                Some(maybe.GetValueOrThrow(), context);
            else
                None(context);
        }

        /// <inheritdoc cref="Match{TE, T}(in Maybe{T}, Func{T, TE}, Func{TE})"/>
        public static TE Match<TE, TKey, TValue>(
            in this Maybe<KeyValuePair<TKey, TValue>> maybe,
            Func<TKey, TValue, TE> Some,
            Func<TE> None
        )
        {
            return maybe.HasValue
                ? Some.Invoke(maybe.GetValueOrThrow().Key, maybe.GetValueOrThrow().Value)
                : None.Invoke();
        }

        /// <inheritdoc cref="Match{TE, T}(in Maybe{T}, Func{T, TE}, Func{TE})"/>
        public static TE Match<TE, TKey, TValue, TContext>(
            in this Maybe<KeyValuePair<TKey, TValue>> maybe,
            Func<TKey, TValue, TContext, TE> Some,
            Func<TContext, TE> None,
            TContext context
        )
        {
            return maybe.HasValue
                ? Some.Invoke(maybe.GetValueOrThrow().Key, maybe.GetValueOrThrow().Value, context)
                : None.Invoke(context);
        }

        /// <inheritdoc cref="Match{T}(in Maybe{T}, Action{T}, Action)"/>
        public static void Match<TKey, TValue>(
            in this Maybe<KeyValuePair<TKey, TValue>> maybe,
            Action<TKey, TValue> Some,
            Action None
        )
        {
            if (maybe.HasValue)
                Some.Invoke(maybe.GetValueOrThrow().Key, maybe.GetValueOrThrow().Value);
            else
                None.Invoke();
        }

        /// <inheritdoc cref="Match{T}(in Maybe{T}, Action{T}, Action)"/>
        public static void Match<TKey, TValue, TContext>(
            in this Maybe<KeyValuePair<TKey, TValue>> maybe,
            Action<TKey, TValue, TContext> Some,
            Action<TContext> None,
            TContext context
        )
        {
            if (maybe.HasValue)
                Some.Invoke(maybe.GetValueOrThrow().Key, maybe.GetValueOrThrow().Value, context);
            else
                None.Invoke(context);
        }
    }
}
