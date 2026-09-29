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
        /// <param name="action">The action to execute with the value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The original <paramref name="maybe"/>.</returns>
		public static Maybe<T> Tap<T>(in this Maybe<T> maybe, Action<T> action)
		{
            if (maybe.HasValue)
                action(maybe.GetValueOrThrow());

            return maybe;
		}
	}
}
