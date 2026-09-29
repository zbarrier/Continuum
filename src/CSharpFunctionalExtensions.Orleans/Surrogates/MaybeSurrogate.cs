using Orleans;

namespace Continuum.CSharpFunctionalExtensions.Orleans;

/// <summary>
///     Orleans surrogate for <see cref="Maybe{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
[GenerateSerializer, Immutable, Alias("Continuum.Maybe`1")]
public struct MaybeSurrogate<T>
{
    /// <summary>Whether a value is present.</summary>
    [Id(0)] public bool HasValue;
    /// <summary>The value, when <see cref="HasValue"/> is true.</summary>
    [Id(1)] public T Value;
}

/// <summary>
///     Converts between <see cref="Maybe{T}"/> and <see cref="MaybeSurrogate{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
[RegisterConverter]
public sealed class MaybeSurrogateConverter<T> : IConverter<Maybe<T>, MaybeSurrogate<T>>
{
    /// <inheritdoc/>
    public Maybe<T> ConvertFromSurrogate(in MaybeSurrogate<T> surrogate) =>
        surrogate.HasValue
            ? Maybe<T>.From(surrogate.Value)
            : Maybe<T>.None;

    /// <inheritdoc/>
    public MaybeSurrogate<T> ConvertToSurrogate(in Maybe<T> value) =>
        value.HasValue
            ? new MaybeSurrogate<T> { HasValue = true, Value = value.Value }
            : new MaybeSurrogate<T> { HasValue = false };
}
