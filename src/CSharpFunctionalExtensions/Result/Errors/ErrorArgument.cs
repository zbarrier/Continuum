#nullable enable

using System.Diagnostics;
using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Identifies the kind of value stored in an <see cref="ErrorArgument"/>.
/// </summary>
public enum ErrorArgumentKind
{
    /// <summary>No value (null).</summary>
    Null = 0,
    /// <summary>A <see cref="string"/> value.</summary>
    String = 1,
    /// <summary>A signed integer value, stored as <see cref="long"/>.</summary>
    Int64 = 2,
    /// <summary>A <see cref="double"/> value.</summary>
    Double = 3,
    /// <summary>A <see cref="decimal"/> value.</summary>
    Decimal = 4,
    /// <summary>A <see cref="bool"/> value.</summary>
    Boolean = 5,
    /// <summary>A <see cref="System.DateTime"/> value.</summary>
    DateTime = 6,
    /// <summary>A <see cref="System.DateTimeOffset"/> value.</summary>
    DateTimeOffset = 7,
    /// <summary>A <see cref="System.TimeSpan"/> value.</summary>
    TimeSpan = 8,
    /// <summary>A <see cref="System.Guid"/> value.</summary>
    Guid = 9,
    /// <summary>A <see cref="System.DateOnly"/> value.</summary>
    DateOnly = 10,
    /// <summary>A <see cref="System.TimeOnly"/> value.</summary>
    TimeOnly = 11,
    /// <summary>A <see cref="System.Numerics.BigInteger"/> value.</summary>
    BigInteger = 12,
}

/// <summary>
///     A typed argument for an error message template.
/// </summary>
/// <remarks>
///     <para>
///         Arguments keep their original type so that they round-trip exactly through serialization and still honor
///         culture-aware formatting and format specifiers such as <c>{0:N2}</c> in message templates.
///     </para>
///     <para>
///         Values convert implicitly from <see cref="string"/>, the integer types, <see cref="double"/>, <see cref="float"/>,
///         <see cref="decimal"/>, <see cref="bool"/>, <see cref="System.DateTime"/>, <see cref="System.DateTimeOffset"/>,
///         <see cref="System.TimeSpan"/>, <see cref="System.Guid"/>, <see cref="System.DateOnly"/>, <see cref="System.TimeOnly"/> and
///         <see cref="System.Numerics.BigInteger"/>. <see cref="FromObject"/> converts any other value to a string using the invariant culture.
///     </para>
/// </remarks>
[DebuggerDisplay("{Kind}: {ToString(),nq}")]
public readonly struct ErrorArgument : IEquatable<ErrorArgument>, IFormattable
{
    private readonly object? _value;

    private ErrorArgument(ErrorArgumentKind kind, object? value)
    {
        Kind = value is null ? ErrorArgumentKind.Null : kind;
        _value = value;
    }

    /// <summary>Gets the kind of value stored in this argument.</summary>
    public ErrorArgumentKind Kind { get; }

    /// <summary>Gets the underlying value, or null when <see cref="Kind"/> is <see cref="ErrorArgumentKind.Null"/>.</summary>
    public object? Value => _value;

    /// <summary>Gets an argument representing null.</summary>
    public static ErrorArgument Null => default;

    /// <summary>Creates a string argument.</summary>
    public static ErrorArgument From(string? value) => new(ErrorArgumentKind.String, value);
    /// <summary>Creates an integer argument.</summary>
    public static ErrorArgument From(long value) => new(ErrorArgumentKind.Int64, value);
    /// <summary>Creates a double argument.</summary>
    public static ErrorArgument From(double value) => new(ErrorArgumentKind.Double, value);
    /// <summary>Creates a decimal argument.</summary>
    public static ErrorArgument From(decimal value) => new(ErrorArgumentKind.Decimal, value);
    /// <summary>Creates a boolean argument.</summary>
    public static ErrorArgument From(bool value) => new(ErrorArgumentKind.Boolean, value);
    /// <summary>Creates a date/time argument.</summary>
    public static ErrorArgument From(DateTime value) => new(ErrorArgumentKind.DateTime, value);
    /// <summary>Creates a date/time offset argument.</summary>
    public static ErrorArgument From(DateTimeOffset value) => new(ErrorArgumentKind.DateTimeOffset, value);
    /// <summary>Creates a time span argument.</summary>
    public static ErrorArgument From(TimeSpan value) => new(ErrorArgumentKind.TimeSpan, value);
    /// <summary>Creates a GUID argument.</summary>
    public static ErrorArgument From(Guid value) => new(ErrorArgumentKind.Guid, value);
    /// <summary>Creates a date argument.</summary>
    public static ErrorArgument From(DateOnly value) => new(ErrorArgumentKind.DateOnly, value);
    /// <summary>Creates a time-of-day argument.</summary>
    public static ErrorArgument From(TimeOnly value) => new(ErrorArgumentKind.TimeOnly, value);
    /// <summary>Creates a big integer argument.</summary>
    public static ErrorArgument From(System.Numerics.BigInteger value) => new(ErrorArgumentKind.BigInteger, value);

    /// <summary>
    ///     Creates an argument from a value of any type. Supported types keep their type; any other value is converted to a
    ///     string using the invariant culture.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The typed argument.</returns>
    public static ErrorArgument FromObject(object? value) => value switch
    {
        null => Null,
        ErrorArgument a => a,
        string s => From(s),
        long l => From(l),
        int i => From(i),
        short s16 => From(s16),
        sbyte s8 => From(s8),
        byte u8 => From(u8),
        ushort u16 => From(u16),
        uint u32 => From(u32),
        ulong u64 when u64 <= long.MaxValue => From((long)u64),
        double d => From(d),
        float f => From(f),
        decimal m => From(m),
        bool b => From(b),
        DateTime dt => From(dt),
        DateTimeOffset dto => From(dto),
        TimeSpan ts => From(ts),
        Guid g => From(g),
        DateOnly d => From(d),
        TimeOnly t => From(t),
        System.Numerics.BigInteger bi => From(bi),
        char c => From(c.ToString()),
        Enum e => From(e.ToString()),
        IFormattable f => From(f.ToString(null, CultureInfo.InvariantCulture)),
        _ => From(value.ToString()),
    };

#pragma warning disable CS1591
    public static implicit operator ErrorArgument(string? value) => From(value);
    public static implicit operator ErrorArgument(int value) => From(value);
    public static implicit operator ErrorArgument(long value) => From(value);
    public static implicit operator ErrorArgument(short value) => From(value);
    public static implicit operator ErrorArgument(byte value) => From(value);
    public static implicit operator ErrorArgument(uint value) => From(value);
    public static implicit operator ErrorArgument(double value) => From(value);
    public static implicit operator ErrorArgument(float value) => From(value);
    public static implicit operator ErrorArgument(decimal value) => From(value);
    public static implicit operator ErrorArgument(bool value) => From(value);
    public static implicit operator ErrorArgument(DateTime value) => From(value);
    public static implicit operator ErrorArgument(DateTimeOffset value) => From(value);
    public static implicit operator ErrorArgument(TimeSpan value) => From(value);
    public static implicit operator ErrorArgument(Guid value) => From(value);
    public static implicit operator ErrorArgument(DateOnly value) => From(value);
    public static implicit operator ErrorArgument(TimeOnly value) => From(value);
    public static implicit operator ErrorArgument(System.Numerics.BigInteger value) => From(value);
#pragma warning restore CS1591

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider) => _value switch
    {
        null => string.Empty,
        IFormattable f => f.ToString(format, formatProvider),
        _ => _value.ToString() ?? string.Empty,
    };

    /// <inheritdoc/>
    public override string ToString() => ToString(null, CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public bool Equals(ErrorArgument other) => Kind == other.Kind && Equals(_value, other._value);
    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ErrorArgument other && Equals(other);
    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, _value);

    /// <summary>Determines whether two arguments are equal.</summary>
    public static bool operator ==(ErrorArgument left, ErrorArgument right) => left.Equals(right);
    /// <summary>Determines whether two arguments are not equal.</summary>
    public static bool operator !=(ErrorArgument left, ErrorArgument right) => !left.Equals(right);

    internal static ErrorArgument[] FromObjects(object[]? values)
    {
        if (values is null || values.Length == 0)
        {
            return [];
        }

        var result = new ErrorArgument[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = FromObject(values[i]);
        }

        return result;
    }

    internal static object?[] ToObjects(ErrorArgument[] values)
    {
        var result = new object?[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = values[i]._value;
        }

        return result;
    }
}
