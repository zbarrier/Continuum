using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
	public static partial class MaybeExtensions
	{
        /// <summary>
        ///     Executes the given action if the <see cref="Maybe{T}"/> produced by <paramref name="maybeTask"/> has a value, then returns the original <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="maybeTask">A value task producing the maybe to inspect.</param>
        /// <param name="valueTask">The asynchronous action to execute with the value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The original <see cref="Maybe{T}"/>.</returns>
		public static async ValueTask<Maybe<T>> Tap<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask> valueTask)
		{
            var maybe = await maybeTask;

            if (maybe.HasValue)
                await valueTask(maybe.GetValueOrThrow());

            return maybe;
		}
	}
}
