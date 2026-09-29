using System;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     The exception thrown when the value of a failed result is accessed.
/// </summary>
public class ResultFailureException : Exception
{
    /// <summary>
    ///     Gets the error of the failed result.
    /// </summary>
    public Error Error { get; }

    internal ResultFailureException(Error error)
        : base(Result.Messages.ValueIsInaccessibleForFailure(error.GetFormattedMessage()))
    {
        Error = error;
    }
}
