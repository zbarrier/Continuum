using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapWithTransactionScope{T, K}(Result{T}, Func{T, K})"/>
        public static ValueTask<Result<K>> MapWithTransactionScope<T, K>(this ValueTask<Result<T>> self, Func<T, ValueTask<K>> f)
            => WithTransactionScope(() => self.Map(f));

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.MapWithTransactionScope{K}(Result, Func{K})"/>
        public static ValueTask<Result<K>> MapWithTransactionScope<K>(this ValueTask<Result> self, Func<ValueTask<K>> f)
            => WithTransactionScope(() => self.Map(f));
    }
}
