using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     This method should be used in linq queries. We recommend using Bind method.
        /// </summary>
        public static ValueTask<Result<TR>> SelectMany<T, TK, TR>(
            this Result<T> result,
            Func<T, ValueTask<Result<TK>>> valueTask,
            Func<T, TK, TR> project)
        {
            return result
                .Bind(valueTask)
                .Map(x => project(result.Value, x));
        }
    }
}