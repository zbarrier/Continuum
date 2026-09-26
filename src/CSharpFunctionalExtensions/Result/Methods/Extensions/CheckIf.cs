using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     If the calling result is a success and the condition is true, the given function is executed and its Result is checked. If this Result is a failure, it is returned. Otherwise, the calling result is returned.
        /// </summary>
        public static Result<T> CheckIf<T>(this Result<T> result, bool condition, Func<T, Result> func)
        {
            if (condition)
                return result.Check(func);
            else
                return result;
        }

        /// <summary>
        ///     If the calling result is a success and the condition is true, the given function is executed and its Result is checked. If this Result is a failure, it is returned. Otherwise, the calling result is returned.
        /// </summary>
        public static Result<T> CheckIf<T, K>(this Result<T> result, bool condition, Func<T, Result<K>> func)
        {
            if (condition)
                return result.Check(func);
            else
                return result;
        }

        /// <summary>
        ///     If the calling result is a success and the predicate is true, the given function is executed and its Result is checked. If this Result is a failure, it is returned. Otherwise, the calling result is returned.
        /// </summary>
        public static Result<T> CheckIf<T>(this Result<T> result, Func<T, bool> predicate, Func<T, Result> func)
        {
            if (result.IsSuccess && predicate(result.Value))
                return result.Check(func);
            else
                return result;
        }

        /// <summary>
        ///     If the calling result is a success and the predicate is true, the given function is executed and its Result is checked. If this Result is a failure, it is returned. Otherwise, the calling result is returned.
        /// </summary>
        public static Result<T> CheckIf<T, K>(this Result<T> result, Func<T, bool> predicate, Func<T, Result<K>> func)
        {
            if (result.IsSuccess && predicate(result.Value))
                return result.Check(func);
            else
                return result;
        }
    }
}