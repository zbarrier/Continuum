using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Combines several results (and any validation error messages) into a single result.
    ///     The returned result will be a failure if any of the input <paramref name="results"/> are failures.</summary>
    /// <param name="results">
    ///     The Results to be combined.</param>
    /// <returns>
    ///     A Result that is a success when all the input <paramref name="results"/> are also successes.</returns>
    public static Result Combine(IEnumerable<Result> results)
    {
        List<Result>? nonValidationErrorResults = null;
        List<Result>? validationErrorResults = null;

        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                if (result.Error is ValidationError)
                {
                    (validationErrorResults ??= new List<Result>()).Add(result);
                }
                else
                {
                    (nonValidationErrorResults ??= new List<Result>()).Add(result);
                }
            }
        }

        if (nonValidationErrorResults is null && validationErrorResults is null)
        {
            return Success();
        }

        if (validationErrorResults is not null)
        {
            var validationFailureResult = Failure(CombineValidationErrors(validationErrorResults));
            if (nonValidationErrorResults is null)
            {
                return validationFailureResult;
            }
            nonValidationErrorResults.Add(validationFailureResult);
        }

        return Failure(SelectHighestPriorityError(nonValidationErrorResults!));
    }

    /// <summary>
    ///     Combines several results (and any validation error messages) into a single result.
    ///     The returned result will be a failure if any of the input <paramref name="results"/> are failures.</summary>
    /// <param name="results">
    ///     The Results to be combined.</param>
    /// <returns>
    ///     A Result that is a success when all the input <paramref name="results"/> are also successes.</returns>
    public static Result Combine<T>(IEnumerable<Result<T>> results)
    {
        List<Result>? nonValidationErrorResults = null;
        List<Result>? validationErrorResults = null;

        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                if (result.Error is ValidationError)
                {
                    (validationErrorResults ??= new List<Result>()).Add(result);
                }
                else
                {
                    (nonValidationErrorResults ??= new List<Result>()).Add(result);
                }
            }
        }

        if (nonValidationErrorResults is null && validationErrorResults is null)
        {
            return Success();
        }

        if (validationErrorResults is not null)
        {
            var validationFailureResult = Failure(CombineValidationErrors(validationErrorResults));
            if (nonValidationErrorResults is null)
            {
                return validationFailureResult;
            }
            nonValidationErrorResults.Add(validationFailureResult);
        }

        return Failure(SelectHighestPriorityError(nonValidationErrorResults!));
    }

    /// <summary>
    ///     Combines several results (and any validation error messages) into a single result.
    ///     The returned result will be a failure if any of the input <paramref name="results"/> are failures.</summary>
    /// <param name="results">
    ///     The Results to be combined.</param>
    /// <returns>
    ///     A Result that is a success when all the input <paramref name="results"/> are also successes.</returns>
    public static Result Combine(params Result[] results)
    {
        List<Result>? nonValidationErrorResults = null;
        List<Result>? validationErrorResults = null;

        var arraySpan = results.AsSpan();
        ref var searchSpace = ref MemoryMarshal.GetReference(arraySpan);
        for (int i = 0; i < arraySpan.Length; i++)
        {
            var result = Unsafe.Add(ref searchSpace, i);
            if (result.IsFailure)
            {
                if (result.Error is ValidationError)
                {
                    (validationErrorResults ??= new List<Result>()).Add(result);
                }
                else
                {
                    (nonValidationErrorResults ??= new List<Result>()).Add(result);
                }
            }
        }

        if (nonValidationErrorResults is null && validationErrorResults is null)
        {
            return Success();
        }

        if (validationErrorResults is not null)
        {
            var validationFailureResult = Failure(CombineValidationErrors(validationErrorResults));
            if (nonValidationErrorResults is null)
            {
                return validationFailureResult;
            }
            nonValidationErrorResults.Add(validationFailureResult);
        }

        return Failure(SelectHighestPriorityError(nonValidationErrorResults!));
    }

    /// <summary>
    ///     Combines several results (and any valiation error messages) into a single result.
    ///     The returned result will be a failure if any of the input <paramref name="results"/> are failures.</summary>
    /// <param name="results">
    ///     The Results to be combined.</param>
    /// <returns>
    ///     A Result that is a success when all the input <paramref name="results"/> are also successes.</returns>
    public static Result Combine<T>(params Result<T>[] results)
    {
        List<Result>? nonValidationErrorResults = null;
        List<Result>? validationErrorResults = null;

        var arraySpan = results.AsSpan();
        ref var searchSpace = ref MemoryMarshal.GetReference(arraySpan);
        for (int i = 0; i < arraySpan.Length; i++)
        {
            var result = Unsafe.Add(ref searchSpace, i);
            if (result.IsFailure)
            {
                if (result.Error is ValidationError)
                {
                    (validationErrorResults ??= new List<Result>()).Add(result);
                }
                else
                {
                    (nonValidationErrorResults ??= new List<Result>()).Add(result);
                }
            }
        }

        if (nonValidationErrorResults is null && validationErrorResults is null)
        {
            return Success();
        }

        if (validationErrorResults is not null)
        {
            var validationFailureResult = Failure(CombineValidationErrors(validationErrorResults));
            if (nonValidationErrorResults is null)
            {
                return validationFailureResult;
            }
            nonValidationErrorResults.Add(validationFailureResult);
        }

        return Failure(SelectHighestPriorityError(nonValidationErrorResults!));
    }

    private static Error SelectHighestPriorityError(List<Result> failedResults)
    {
        if (failedResults.Count == 0)
        {
            throw new ArgumentException("Must have at least one failed result.", nameof(failedResults));
        }

        Span<Result> resultsAsSpan = CollectionsMarshal.AsSpan(failedResults);
        ref var resultSearchSpace = ref MemoryMarshal.GetReference(resultsAsSpan);

        Error error = Unsafe.Add(ref resultSearchSpace, 0).Error;
        for (int i = 1; i < resultsAsSpan.Length; i++)
        {
            var nextResult = Unsafe.Add(ref resultSearchSpace, i);
            if (nextResult.Error.CompareTo(error) < 0)
            {
                error = nextResult.Error;
            }
        }

        return error;
    }
}
