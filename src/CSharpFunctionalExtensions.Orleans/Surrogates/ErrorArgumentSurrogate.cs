using System.Globalization;
using System.Numerics;

using Orleans;

namespace Continuum.CSharpFunctionalExtensions.Orleans;

/// <summary>
///     Orleans surrogate for <see cref="ErrorArgument"/>. The value is stored as an invariant string together with its
///     <see cref="ErrorArgumentKind"/> so that it round-trips with its original type.
/// </summary>
[GenerateSerializer, Immutable, Alias("Continuum.ErrorArgument")]
public struct ErrorArgumentSurrogate
{
    /// <summary>The kind of the value.</summary>
    [Id(0)] public ErrorArgumentKind Kind;
    /// <summary>The value formatted with the invariant culture, or null.</summary>
    [Id(1)] public string? Value;
}

/// <summary>
///     Converts between <see cref="ErrorArgument"/> and <see cref="ErrorArgumentSurrogate"/>.
/// </summary>
[RegisterConverter]
public sealed class ErrorArgumentSurrogateConverter : IConverter<ErrorArgument, ErrorArgumentSurrogate>
{
    /// <inheritdoc/>
    public ErrorArgument ConvertFromSurrogate(in ErrorArgumentSurrogate surrogate)
    {
        if (surrogate.Kind == ErrorArgumentKind.Null || surrogate.Value is null)
        {
            return ErrorArgument.Null;
        }

        var raw = surrogate.Value;
        var inv = CultureInfo.InvariantCulture;
        return surrogate.Kind switch
        {
            ErrorArgumentKind.String => ErrorArgument.From(raw),
            ErrorArgumentKind.Int64 => ErrorArgument.From(long.Parse(raw, inv)),
            ErrorArgumentKind.Double => ErrorArgument.From(double.Parse(raw, NumberStyles.Float, inv)),
            ErrorArgumentKind.Decimal => ErrorArgument.From(decimal.Parse(raw, NumberStyles.Float, inv)),
            ErrorArgumentKind.Boolean => ErrorArgument.From(bool.Parse(raw)),
            ErrorArgumentKind.DateTime => ErrorArgument.From(DateTime.Parse(raw, inv, DateTimeStyles.RoundtripKind)),
            ErrorArgumentKind.DateTimeOffset => ErrorArgument.From(DateTimeOffset.Parse(raw, inv, DateTimeStyles.RoundtripKind)),
            ErrorArgumentKind.TimeSpan => ErrorArgument.From(TimeSpan.ParseExact(raw, "c", inv)),
            ErrorArgumentKind.Guid => ErrorArgument.From(Guid.Parse(raw)),
            ErrorArgumentKind.DateOnly => ErrorArgument.From(DateOnly.ParseExact(raw, "O", inv)),
            ErrorArgumentKind.TimeOnly => ErrorArgument.From(TimeOnly.ParseExact(raw, "O", inv)),
            ErrorArgumentKind.BigInteger => ErrorArgument.From(BigInteger.Parse(raw, inv)),
            _ => throw new NotSupportedException($"Unsupported error argument kind '{surrogate.Kind}'."),
        };
    }

    /// <inheritdoc/>
    public ErrorArgumentSurrogate ConvertToSurrogate(in ErrorArgument value)
    {
        var inv = CultureInfo.InvariantCulture;
        var raw = value.Value switch
        {
            null => null,
            string s => s,
            double d => d.ToString("R", inv),
            DateTime dt => dt.ToString("O", inv),
            DateTimeOffset dto => dto.ToString("O", inv),
            TimeSpan ts => ts.ToString("c", inv),
            DateOnly d => d.ToString("O", inv),
            TimeOnly t => t.ToString("O", inv),
            IFormattable f => f.ToString(null, inv),
            var other => other.ToString(),
        };

        return new ErrorArgumentSurrogate { Kind = value.Kind, Value = raw };
    }
}
