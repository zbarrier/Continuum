using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     This method should be used in linq queries. We recommend using Map method.
        /// </summary>
        public static ValueTask<Maybe<K>> Select<T, K>(this ValueTask<Maybe<T>> maybeTask, Func<T, K> selector)
        {
            return maybeTask.Map(selector);
        }
    }
}
