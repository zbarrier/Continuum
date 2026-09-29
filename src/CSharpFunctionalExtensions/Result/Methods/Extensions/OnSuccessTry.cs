using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     When <paramref name="result"/> is a success, executes <paramref name="action"/> and converts any thrown exception into a failure.
        /// </summary>
        /// <param name="result">The source result.</param>
        /// <param name="action">The action to execute on success.</param>
        /// <param name="errorHandler">Maps a caught exception to an <see cref="Error"/>; when <see langword="null"/>, the default try handler is used.</param>
        /// <returns>The original failure, a success, or a failure produced from the caught exception.</returns>
        public static Result OnSuccessTry(this Result result, Action action, Func<Exception, Error>? errorHandler = null)
        {
            return result.IsFailure
                ? result
                : Result.Try(action, errorHandler);
        }

        /// <inheritdoc cref="OnSuccessTry(Result, Action, Func{Exception, Error})"/>
        public static Result OnSuccessTry<T>(this Result<T> result, Action<T> action, Func<Exception, Error>? errorHandler = null)
        {
            return result.IsFailure
                ? Result.Failure(result.Error)
                : Result.Try(() => action(result.Value), errorHandler);
        }
    }
}
