using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
	public static partial class MaybeExtensions
	{
        /// <summary>
        ///     Executes the given action if <paramref name="maybe"/> has no value, then returns the original <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="maybe">The maybe to inspect.</param>
        /// <param name="valueTask">The asynchronous action to execute when there is no value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The original <paramref name="maybe"/>.</returns>
		public static async ValueTask<Maybe<T>> TapNoValue<T>(this Maybe<T> maybe, Func<ValueTask> valueTask)
		{
            if (maybe.HasNoValue)
                await valueTask();

            return maybe;
		}
	}
}
