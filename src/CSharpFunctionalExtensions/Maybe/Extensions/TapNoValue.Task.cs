using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
	public static partial class MaybeExtensions
	{
        /// <summary>
        ///     Executes the given action if the <see cref="Maybe{T}"/> produced by <paramref name="maybeTask"/> has no value, then returns the original <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="maybeTask">A task producing the maybe to inspect.</param>
        /// <param name="asyncAction">The asynchronous action to execute when there is no value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The original <see cref="Maybe{T}"/>.</returns>
		public static async Task<Maybe<T>> TapNoValue<T>(this Task<Maybe<T>> maybeTask, Func<Task> asyncAction)
		{
            var maybe = await maybeTask.DefaultAwait();

            if (maybe.HasNoValue)
                await asyncAction().DefaultAwait();

            return maybe;
		}
	}
}
