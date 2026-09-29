using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
	public static partial class MaybeExtensions
	{
        /// <summary>
        ///     Executes the given action if the <see cref="Maybe{T}"/> produced by <paramref name="maybeTask"/> has no value, then returns the original <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="maybeTask">A value task producing the maybe to inspect.</param>
        /// <param name="action">The action to execute when there is no value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The original <see cref="Maybe{T}"/>.</returns>
		public static async ValueTask<Maybe<T>> TapNoValue<T>(this ValueTask<Maybe<T>> maybeTask, Action action)
		{
            var maybe = await maybeTask;

            if (maybe.HasNoValue)
                action();

            return maybe;
		}
	}
}
