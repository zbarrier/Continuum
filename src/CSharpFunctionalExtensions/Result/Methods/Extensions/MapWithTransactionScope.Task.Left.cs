using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapWithTransactionScope{T, K}(Result{T}, Func{T, K})"/>
        public static Task<Result<K>> MapWithTransactionScope<T, K>(this Task<Result<T>> self, Func<T, K> f)
            => WithTransactionScope(() => self.Map(f));

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapWithTransactionScope{K}(Result, Func{K})"/>
        public static Task<Result<K>> MapWithTransactionScope<K>(this Task<Result> self, Func<K> f)
            => WithTransactionScope(() => self.Map(f));
    }
}
