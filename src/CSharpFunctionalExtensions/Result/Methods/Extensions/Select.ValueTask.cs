using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     This method should be used in linq queries. We recommend using Map method.
        /// </summary>
        public static ValueTask<Result<K>> Select<T, K>(this ValueTask<Result<T>> resultTask, Func<T, K> selector)
        {
            return resultTask.Map(selector);
        }
    }
}
