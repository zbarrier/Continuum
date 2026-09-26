using System;

namespace Continuum.CSharpFunctionalExtensions;

public class ResultSuccessException : Exception
{
    internal ResultSuccessException()
        : base(Result.Messages.ErrorIsInaccessibleForSuccess)
    {
    }
}
