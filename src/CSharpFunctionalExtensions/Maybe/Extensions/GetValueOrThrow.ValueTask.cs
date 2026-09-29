using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Maybe{T}.GetValueOrThrow(string)"/>
        public static async ValueTask<T> GetValueOrThrow<T>(this ValueTask<Maybe<T>> maybeTask)
        {
            var maybe = await maybeTask;
            return maybe.GetValueOrThrow();
        }

        /// <summary>
        ///     Returns <paramref name="maybeTask" />'s inner value if it has one, otherwise throws an InvalidOperationException
        ///     with <paramref name="errorMessage" />
        /// </summary>
        /// <exception cref="InvalidOperationException">Maybe has no value.</exception>
        public static async ValueTask<T> GetValueOrThrow<T>(this ValueTask<Maybe<T>> maybeTask, string errorMessage)
        {
            var maybe = await maybeTask;
            return maybe.GetValueOrThrow(errorMessage);
        }
    }
}
