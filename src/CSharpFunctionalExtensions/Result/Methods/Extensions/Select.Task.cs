using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     This method should be used in linq queries. We recommend using Map method.
        /// </summary>
        public static Task<Result<K>> Select<T, K>(this Task<Result<T>> resultTask, Func<T, K> selector)
        {
            return resultTask.Map(selector);
        }
    }
}
