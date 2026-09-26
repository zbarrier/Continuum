using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     This method should be used in linq queries. We recommend using Bind method.
        /// </summary>
        public static async Task<Result<TR>> SelectMany<T, TK, TR>(
            this Task<Result<T>> resultTask,
            Func<T, Task<Result<TK>>> func,
            Func<T, TK, TR> project)
        {
            Result<T> result = await resultTask.DefaultAwait();
            return await result.SelectMany(func, project).DefaultAwait();
        }
    }
}