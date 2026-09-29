using Orleans;

namespace Continuum.CSharpFunctionalExtensions.Orleans;

/// <summary>
///     Orleans surrogate for <see cref="Result"/>.
/// </summary>
/// <remarks>
///     <see cref="Error"/> is serialized polymorphically, so any error type with a registered Orleans serializer or
///     surrogate (such as <see cref="RequestError"/> and <see cref="ValidationError"/>) round-trips.
/// </remarks>
[GenerateSerializer, Immutable, Alias("Continuum.Result")]
public struct ResultSurrogate
{
    /// <summary>Whether the result is a failure.</summary>
    [Id(0)] public bool IsFailure;
    /// <summary>The error, when <see cref="IsFailure"/> is true.</summary>
    [Id(1)] public Error? Error;
}

/// <summary>
///     Converts between <see cref="Result"/> and <see cref="ResultSurrogate"/>.
/// </summary>
[RegisterConverter]
public sealed class ResultSurrogateConverter : IConverter<Result, ResultSurrogate>
{
    /// <inheritdoc/>
    public Result ConvertFromSurrogate(in ResultSurrogate surrogate) =>
        surrogate.IsFailure
            ? Result.Failure(surrogate.Error!)
            : Result.Success();

    /// <inheritdoc/>
    public ResultSurrogate ConvertToSurrogate(in Result value) =>
        value.IsFailure
            ? new ResultSurrogate { IsFailure = true, Error = value.Error }
            : new ResultSurrogate { IsFailure = false };
}

/// <summary>
///     Orleans surrogate for <see cref="Result{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
[GenerateSerializer, Immutable, Alias("Continuum.Result`1")]
public struct ResultSurrogate<T>
{
    /// <summary>Whether the result is a failure.</summary>
    [Id(0)] public bool IsFailure;
    /// <summary>The error, when <see cref="IsFailure"/> is true.</summary>
    [Id(1)] public Error? Error;
    /// <summary>The value, when <see cref="IsFailure"/> is false.</summary>
    [Id(2)] public T Value;
}

/// <summary>
///     Converts between <see cref="Result{T}"/> and <see cref="ResultSurrogate{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
[RegisterConverter]
public sealed class ResultSurrogateConverter<T> : IConverter<Result<T>, ResultSurrogate<T>>
{
    /// <inheritdoc/>
    public Result<T> ConvertFromSurrogate(in ResultSurrogate<T> surrogate) =>
        surrogate.IsFailure
            ? Result.Failure<T>(surrogate.Error!)
            : Result.Success(surrogate.Value);

    /// <inheritdoc/>
    public ResultSurrogate<T> ConvertToSurrogate(in Result<T> value) =>
        value.IsFailure
            ? new ResultSurrogate<T> { IsFailure = true, Error = value.Error }
            : new ResultSurrogate<T> { IsFailure = false, Value = value.Value };
}
