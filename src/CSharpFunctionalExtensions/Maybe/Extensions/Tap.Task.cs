using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
	public static partial class MaybeExtensions
	{
        /// <summary>
        ///     Executes the given action if the <see cref="Maybe{T}"/> produced by <paramref name="maybeTask"/> has a value, then returns the original <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="maybeTask">A task producing the maybe to inspect.</param>
        /// <param name="asyncAction">The asynchronous action to execute with the value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The original <see cref="Maybe{T}"/>.</returns>
		public static async Task<Maybe<T>> Tap<T>(this Task<Maybe<T>> maybeTask, Func<T, Task> asyncAction)
		{
            var maybe = await maybeTask.DefaultAwait();

            if (maybe.HasValue)
                await asyncAction(maybe.GetValueOrThrow()).DefaultAwait();

            return maybe;
		}
	}
}
