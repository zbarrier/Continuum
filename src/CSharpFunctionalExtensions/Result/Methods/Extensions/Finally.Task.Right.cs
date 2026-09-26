using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsRightOperand
    {
        /// <summary>
        ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
        /// </summary>
        public static Task<T> Finally<T>(this Result result, Func<Result, Task<T>> func)
          => func(result);

        /// <summary>
        ///     Passes the result to the given function (regardless of success/failure state) to yield a final output value.
        /// </summary>
        public static Task<K> Finally<T, K>(this Result<T> result, Func<Result<T>, Task<K>> func)
          => func(result);
    }
}
