using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Maybe{T}.GetValueOrThrow(string)"/>
        public static async Task<T> GetValueOrThrow<T>(this Task<Maybe<T>> maybeTask)
        {
            var maybe = await maybeTask.DefaultAwait();
            return maybe.GetValueOrThrow();
        }

        /// <summary>
        ///     Returns <paramref name="maybeTask" />'s inner value if it has one, otherwise throws an InvalidOperationException
        ///     with <paramref name="errorMessage" />
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Maybe has no value.</exception>
        public static async Task<T> GetValueOrThrow<T>(this Task<Maybe<T>> maybeTask, string errorMessage)
        {
            var maybe = await maybeTask.DefaultAwait();
            return maybe.GetValueOrThrow(errorMessage);
        }
    }
}
