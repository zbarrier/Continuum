using System;

namespace Continuum.CSharpFunctionalExtensions;

public class ResultFailureException : Exception
{
    public Error Error { get; }

    internal ResultFailureException(Error error)
        : base(Result.Messages.ValueIsInaccessibleForFailure(error.GetFormattedMessage()))
    {
        Error = error;
    }
}
