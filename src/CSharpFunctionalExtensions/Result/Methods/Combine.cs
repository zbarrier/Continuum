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
        var nonValidationErrorResults = new List<Result>();
        var validationErrorResults = new List<Result>();

        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                if (result.Error is ValidationError)
                {
                    validationErrorResults.Add(result);
                }
                else
                {
                    nonValidationErrorResults.Add(result);
                }
            }
        }

        if (nonValidationErrorResults.Count == 0 && validationErrorResults.Count == 0)
        {
            return Success();
        }

        if (validationErrorResults.Count > 0)
        {
            var validationFailureResult = Failure(CombineValidationErrors(validationErrorResults));
            if (nonValidationErrorResults.Count == 0)
            {
                return validationFailureResult;
            }
            nonValidationErrorResults.Add(validationFailureResult);
        }

        return Failure(SelectHighestPriorityError(nonValidationErrorResults));
    }

    /// <summary>
    ///     Combines several results (and any validation error messages) into a single result.
    ///     The returned result will be a failure if any of the input <paramref name="results"/> are failures.</summary>
    /// <param name="results">
    ///     The Results to be combined.</param>
    /// <param name="errorMessagesSeparator">
    ///     A string that is used to separate any concatenated error messages. If omitted, the default <see cref="Result.Configuration.ErrorMessagesSeparator" /> is used.</param>
    /// <returns>
    ///     A Result that is a success when all the input <paramref name="results"/> are also successes.</returns>
    public static Result Combine<T>(IEnumerable<Result<T>> results)
    {
        var nonValidationErrorResults = new List<Result>();
        var validationErrorResults = new List<Result>();

        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                if (result.Error is ValidationError)
                {
                    validationErrorResults.Add(result);
                }
                else
                {
                    nonValidationErrorResults.Add(result);
                }
            }
        }

        if (nonValidationErrorResults.Count == 0 && validationErrorResults.Count == 0)
        {
            return Success();
        }

        if (validationErrorResults.Count > 0)
        {
            var validationFailureResult = Failure(CombineValidationErrors(validationErrorResults));
            if (nonValidationErrorResults.Count == 0)
            {
                return validationFailureResult;
            }
            nonValidationErrorResults.Add(validationFailureResult);
        }

        return Failure(SelectHighestPriorityError(nonValidationErrorResults));
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
        var nonValidationErrorResults = new List<Result>();
        var validationErrorResults = new List<Result>();

        var arraySpan = results.AsSpan();
        ref var searchSpace = ref MemoryMarshal.GetReference(arraySpan);
        for (int i = 0; i < arraySpan.Length; i++)
        {
            var result = Unsafe.Add(ref searchSpace, i);
            if (result.IsFailure)
            {
                if (result.Error is ValidationError)
                {
                    validationErrorResults.Add(result);
                }
                else
                {
                    nonValidationErrorResults.Add(result);
                }
            }
        }

        if (nonValidationErrorResults.Count == 0 && validationErrorResults.Count == 0)
        {
            return Success();
        }

        if (validationErrorResults.Count > 0)
        {
            var validationFailureResult = Failure(CombineValidationErrors(validationErrorResults));
            if (nonValidationErrorResults.Count == 0)
            {
                return validationFailureResult;
            }
            nonValidationErrorResults.Add(validationFailureResult);
        }

        return Failure(SelectHighestPriorityError(nonValidationErrorResults));
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
        var nonValidationErrorResults = new List<Result>();
        var validationErrorResults = new List<Result>();

        var arraySpan = results.AsSpan();
        ref var searchSpace = ref MemoryMarshal.GetReference(arraySpan);
        for (int i = 0; i < arraySpan.Length; i++)
        {
            var result = Unsafe.Add(ref searchSpace, i);
            if (result.IsFailure)
            {
                if (result.Error is ValidationError)
                {
                    validationErrorResults.Add(result);
                }
                else
                {
                    nonValidationErrorResults.Add(result);
                }
            }
        }

        if (nonValidationErrorResults.Count == 0 && validationErrorResults.Count == 0)
        {
            return Success();
        }

        if (validationErrorResults.Count > 0)
        {
            var validationFailureResult = Failure(CombineValidationErrors(validationErrorResults));
            if (nonValidationErrorResults.Count == 0)
            {
                return validationFailureResult;
            }
            nonValidationErrorResults.Add(validationFailureResult);
        }

        return Failure(SelectHighestPriorityError(nonValidationErrorResults));
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
            if (nextResult.Error.PriorityCode < error.PriorityCode)
            {
                error = nextResult.Error;
            }
        }

        return error;
    }
}
