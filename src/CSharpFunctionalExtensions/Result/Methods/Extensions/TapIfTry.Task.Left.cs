using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
        /// <summary>
        ///     Executes the given action if the calling result is a success and the condition is true. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result> TapIfTry(this Task<Result> resultTask, bool condition, Action action)
        {
            var result = await resultTask.DefaultAwait();
            return result.TapIfTry(condition, action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success and the condition is true. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result<T>> TapIfTry<T>(this Task<Result<T>> resultTask, bool condition, Action action)
        {
            var result = await resultTask.DefaultAwait();
            return result.TapIfTry(condition, action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success and the condition is true. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result<T>> TapIfTry<T>(this Task<Result<T>> resultTask, bool condition, Action<T> action)
        {
            var result = await resultTask.DefaultAwait();
            return result.TapIfTry(condition, action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success and the predicate is true. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result<T>> TapIfTry<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Action action)
        {
            var result = await resultTask.DefaultAwait();
            return result.TapIfTry(predicate, action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success and the predicate is true. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result<T>> TapIfTry<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Action<T> action)
        {
            var result = await resultTask.DefaultAwait();
            return result.TapIfTry(predicate, action);
        }
    }
}
