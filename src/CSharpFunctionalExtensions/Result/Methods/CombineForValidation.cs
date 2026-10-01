using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Combines several results (and any validation error messages) into a single result.
    ///     The returned result will be a failure if any of the input <paramref name="results"/> are failures.
    ///     Every failed result must carry a <see cref="ValidationError"/>; use <see cref="Combine(IEnumerable{Result})"/> for mixed error types.</summary>
    /// <param name="results">
    ///     The Results to be combined.</param>
    /// <returns>
    ///     A Result that is a success when all the input <paramref name="results"/> are also successes.</returns>
    /// <exception cref="ArgumentException">A failed result carries an error that is not a <see cref="ValidationError"/>.</exception>
    public static Result CombineForValidation(IEnumerable<Result> results)
    {
        var failedResults = new List<Result>();
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                failedResults.Add(result);
            }
        }

        return failedResults.Count == 0
            ? Success()
            : Failure(CombineValidationErrors(failedResults));
    }

    /// <summary>
    ///     Combines several results (and any validation error messages) into a single result.
    ///     The returned result will be a failure if any of the input <paramref name="results"/> are failures.
    ///     Every failed result must carry a <see cref="ValidationError"/>; use <see cref="Combine(IEnumerable{Result})"/> for mixed error types.</summary>
    /// <param name="results">
    ///     The Results to be combined.</param>
    /// <returns>
    ///     A Result that is a success when all the input <paramref name="results"/> are also successes.</returns>
    /// <exception cref="ArgumentException">A failed result carries an error that is not a <see cref="ValidationError"/>.</exception>
    public static Result CombineForValidation<T>(IEnumerable<Result<T>> results)
    {
        var failedResults = new List<Result>();
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                failedResults.Add(result);
            }
        }

        return failedResults.Count == 0
            ? Success()
            : Failure(CombineValidationErrors(failedResults));
    }

    /// <summary>
    ///     Combines several results (and any validation error messages) into a single result.
    ///     The returned result will be a failure if any of the input <paramref name="results"/> are failures.
    ///     Every failed result must carry a <see cref="ValidationError"/>; use <see cref="Combine(IEnumerable{Result})"/> for mixed error types.</summary>
    /// <param name="results">
    ///     The Results to be combined.</param>
    /// <returns>
    ///     A Result that is a success when all the input <paramref name="results"/> are also successes.</returns>
    /// <exception cref="ArgumentException">A failed result carries an error that is not a <see cref="ValidationError"/>.</exception>
    public static Result CombineForValidation(params Result[] results)
    {
        var failedResults = new List<Result>();

        var arraySpan = results.AsSpan();
        ref var searchSpace = ref MemoryMarshal.GetReference(arraySpan);
        for (int i = 0; i < arraySpan.Length; i++)
        {
            var result = Unsafe.Add(ref searchSpace, i);
            if (result.IsFailure)
            {
                failedResults.Add(result);
            }
        }

        return failedResults.Count == 0
            ? Success()
            : Failure(CombineValidationErrors(failedResults));
    }

    /// <summary>
    ///     Combines several results (and any validation error messages) into a single result.
    ///     The returned result will be a failure if any of the input <paramref name="results"/> are failures.
    ///     Every failed result must carry a <see cref="ValidationError"/>; use <see cref="Combine(IEnumerable{Result})"/> for mixed error types.</summary>
    /// <param name="results">
    ///     The Results to be combined.</param>
    /// <returns>
    ///     A Result that is a success when all the input <paramref name="results"/> are also successes.</returns>
    /// <exception cref="ArgumentException">A failed result carries an error that is not a <see cref="ValidationError"/>.</exception>
    public static Result CombineForValidation<T>(params Result<T>[] results)
    {
        var failedResults = new List<Result>();

        var arraySpan = results.AsSpan();
        ref var searchSpace = ref MemoryMarshal.GetReference(arraySpan);
        for (int i = 0; i < arraySpan.Length; i++)
        {
            var result = Unsafe.Add(ref searchSpace, i);
            if (result.IsFailure)
            {
                failedResults.Add(result);
            }
        }

        return failedResults.Count == 0
            ? Success()
            : Failure(CombineValidationErrors(failedResults));
    }

    private static ValidationError CombineValidationErrors(List<Result> failedResults)
    {
        if (failedResults.Count == 0)
        {
            throw new ArgumentException("Must have at least one failed result.", nameof(failedResults));
        }

        var listSpan = CollectionsMarshal.AsSpan(failedResults);
        int count = 0;
        foreach (var failedResult in listSpan)
        {
            if (failedResult.Error is not ValidationError validationError)
            {
                throw new ArgumentException("Only validation errors are expected.", nameof(failedResults));
            }
            count += validationError.Entries.Length;
        }

        var allEntries = ImmutableArray.CreateBuilder<ValidationErrorEntry>(count);
        foreach (var failedResult in listSpan)
        {
            allEntries.AddRange(((ValidationError)failedResult.Error).Entries);
        }

        return new ValidationError(allEntries.MoveToImmutable());
    }
}
