using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes <c>Map</c> inside a <see cref="System.Transactions.TransactionScope"/>, completing the scope only when the resulting operation succeeds.
        /// </summary>
        /// <typeparam name="T">The type of the source value.</typeparam>
        /// <typeparam name="K">The type of the mapped value.</typeparam>
        /// <param name="self">The source result.</param>
        /// <param name="f">The mapping function.</param>
        /// <returns>The mapped result.</returns>
        public static Result<K> MapWithTransactionScope<T, K>(this Result<T> self, Func<T, K> f)
            => WithTransactionScope(() => self.Map(f));

        /// <inheritdoc cref="MapWithTransactionScope{T, K}(Result{T}, Func{T, K})"/>
        public static Result<K> MapWithTransactionScope<K>(this Result self, Func<K> f)
            => WithTransactionScope(() => self.Map(f));
    }
}
