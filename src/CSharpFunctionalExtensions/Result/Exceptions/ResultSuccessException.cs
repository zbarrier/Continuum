using System;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     The exception thrown when the error of a successful result is accessed.
/// </summary>
public class ResultSuccessException : Exception
{
    internal ResultSuccessException()
        : base(Result.Messages.ErrorIsInaccessibleForSuccess)
    {
    }
}
