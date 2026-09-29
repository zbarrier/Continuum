using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
	public static partial class MaybeExtensions
	{
        /// <summary>
        ///     Executes the given action if <paramref name="maybe"/> has a value, then returns the original <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="maybe">The maybe to inspect.</param>
        /// <param name="asyncAction">The asynchronous action to execute with the value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The original <paramref name="maybe"/>.</returns>
		public static async Task<Maybe<T>> Tap<T>(this Maybe<T> maybe, Func<T, Task> asyncAction)
		{
            if (maybe.HasValue)
                await asyncAction(maybe.GetValueOrThrow()).DefaultAwait();

            return maybe;
		}
	}
}
