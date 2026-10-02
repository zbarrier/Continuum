using System.Text.Json.Serialization;

namespace Continuum.Domain.Orleans;

/// <summary>Base record for a strongly typed value object that wraps a <see cref="byte"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record ByteValueObject<T> where T : ByteValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected ByteValueObject(byte value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected byte Value { get; init; }

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(ByteValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="byte"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="byte"/> representation of the value.</returns>
    public static implicit operator byte(ByteValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="byte"/>.</summary>
    /// <param name="value">The <see cref="byte"/> representation of the value.</param>
    public void Deconstruct(out byte value) => value = Value;

    /// <summary>Determines whether <paramref name="left"/> is less than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(ByteValueObject<T> left, ByteValueObject<T> right)
        => left.Value < right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(ByteValueObject<T> left, ByteValueObject<T> right)
        => left.Value > right.Value;

    /// <summary>Determines whether <paramref name="left"/> is less than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(ByteValueObject<T> left, ByteValueObject<T> right)
        => left.Value <= right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(ByteValueObject<T> left, ByteValueObject<T> right)
        => left.Value >= right.Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="sbyte"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record SByteValueObject<T> where T : SByteValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected SByteValueObject(sbyte value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected sbyte Value { get; init; }

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(SByteValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="sbyte"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="sbyte"/> representation of the value.</returns>
    public static implicit operator sbyte(SByteValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="sbyte"/>.</summary>
    /// <param name="value">The <see cref="sbyte"/> representation of the value.</param>
    public void Deconstruct(out sbyte value) => value = Value;

    /// <summary>Determines whether <paramref name="left"/> is less than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(SByteValueObject<T> left, SByteValueObject<T> right)
        => left.Value < right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(SByteValueObject<T> left, SByteValueObject<T> right)
        => left.Value > right.Value;

    /// <summary>Determines whether <paramref name="left"/> is less than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(SByteValueObject<T> left, SByteValueObject<T> right)
        => left.Value <= right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(SByteValueObject<T> left, SByteValueObject<T> right)
        => left.Value >= right.Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="DateOnly"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record DateOnlyValueObject<T> where T : DateOnlyValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected DateOnlyValueObject(DateOnly value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected DateOnly Value { get; init; }

    // Could cause issues with serialization if ToString() is used??
    // Can be overridden in the concrete class to provide the desired format.
    // public override string ToString() => Value.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(DateOnlyValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="DateOnly"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="DateOnly"/> representation of the value.</returns>
    public static implicit operator DateOnly(DateOnlyValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="DateOnly"/>.</summary>
    /// <param name="value">The <see cref="DateOnly"/> representation of the value.</param>
    public void Deconstruct(out DateOnly value) => value = Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="DateTime"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record DateTimeValueObject<T> where T : DateTimeValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected DateTimeValueObject(DateTime value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected DateTime Value { get; init; }

    // Could cause issues with serialization if ToString() is used??
    // Can be overridden in the concrete class to provide the desired format.
    // public override string ToString() => Value.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(DateTimeValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="DateTime"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="DateTime"/> representation of the value.</returns>
    public static implicit operator DateTime(DateTimeValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="DateTime"/>.</summary>
    /// <param name="value">The <see cref="DateTime"/> representation of the value.</param>
    public void Deconstruct(out DateTime value) => value = Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="Guid"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record GuidValueObject<T> where T : GuidValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected GuidValueObject(Guid value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected Guid Value { get; init; }

    /// <inheritdoc/>
    public sealed override string ToString() => Value.ToString("D");

    /// <summary>Converts the value object to a <see cref="Guid"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="Guid"/> representation of the value.</returns>
    public static implicit operator Guid(GuidValueObject<T> self) => self.Value;

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(GuidValueObject<T> self) => self.ToString();

    /// <summary>Deconstructs the value object into a <see cref="Guid"/>.</summary>
    /// <param name="value">The <see cref="Guid"/> representation of the value.</param>
    public void Deconstruct(out Guid value) => value = Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="string"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record StringValueObject<T> where T : StringValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected StringValueObject(string value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected string Value { get; init; }

    /// <inheritdoc/>
    public sealed override string ToString() => Value;

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(StringValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="TimeSpan"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record TimeSpanValueObject<T> where T : TimeSpanValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected TimeSpanValueObject(TimeSpan value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected TimeSpan Value { get; init; }

    // Could cause issues with serialization if ToString() is used??
    // Can be overridden in the concrete class to provide the desired format.
    // public override string ToString() => Value.ToString("c");

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(TimeSpanValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="TimeSpan"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="TimeSpan"/> representation of the value.</returns>
    public static implicit operator TimeSpan(TimeSpanValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="TimeSpan"/>.</summary>
    /// <param name="value">The <see cref="TimeSpan"/> representation of the value.</param>
    public void Deconstruct(out TimeSpan value) => value = Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="int"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record Int32ValueObject<T> where T : Int32ValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected Int32ValueObject(int value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected int Value { get; init; }

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(Int32ValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="int"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="int"/> representation of the value.</returns>
    public static implicit operator int(Int32ValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="int"/>.</summary>
    /// <param name="value">The <see cref="int"/> representation of the value.</param>
    public void Deconstruct(out int value) => value = Value;

    /// <summary>Determines whether <paramref name="left"/> is less than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(Int32ValueObject<T> left, Int32ValueObject<T> right)
        => left.Value < right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(Int32ValueObject<T> left, Int32ValueObject<T> right)
        => left.Value > right.Value;

    /// <summary>Determines whether <paramref name="left"/> is less than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(Int32ValueObject<T> left, Int32ValueObject<T> right)
        => left.Value <= right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(Int32ValueObject<T> left, Int32ValueObject<T> right)
        => left.Value >= right.Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="uint"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record UInt32ValueObject<T> where T : UInt32ValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected UInt32ValueObject(uint value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected uint Value { get; init; }

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(UInt32ValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="uint"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="uint"/> representation of the value.</returns>
    public static implicit operator uint(UInt32ValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="uint"/>.</summary>
    /// <param name="value">The <see cref="uint"/> representation of the value.</param>
    public void Deconstruct(out uint value) => value = Value;

    /// <summary>Determines whether <paramref name="left"/> is less than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(UInt32ValueObject<T> left, UInt32ValueObject<T> right)
        => left.Value < right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(UInt32ValueObject<T> left, UInt32ValueObject<T> right)
        => left.Value > right.Value;

    /// <summary>Determines whether <paramref name="left"/> is less than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(UInt32ValueObject<T> left, UInt32ValueObject<T> right)
        => left.Value <= right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(UInt32ValueObject<T> left, UInt32ValueObject<T> right)
        => left.Value >= right.Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="long"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record Int64ValueObject<T> where T : Int64ValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected Int64ValueObject(long value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected long Value { get; init; }

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(Int64ValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="long"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="long"/> representation of the value.</returns>
    public static implicit operator long(Int64ValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="long"/>.</summary>
    /// <param name="value">The <see cref="long"/> representation of the value.</param>
    public void Deconstruct(out long value) => value = Value;

    /// <summary>Determines whether <paramref name="left"/> is less than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(Int64ValueObject<T> left, Int64ValueObject<T> right)
        => left.Value < right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(Int64ValueObject<T> left, Int64ValueObject<T> right)
        => left.Value > right.Value;

    /// <summary>Determines whether <paramref name="left"/> is less than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(Int64ValueObject<T> left, Int64ValueObject<T> right)
        => left.Value <= right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(Int64ValueObject<T> left, Int64ValueObject<T> right)
        => left.Value >= right.Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="ulong"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record UInt64ValueObject<T> where T : UInt64ValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected UInt64ValueObject(ulong value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected ulong Value { get; init; }

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(UInt64ValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="ulong"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="ulong"/> representation of the value.</returns>
    public static implicit operator ulong(UInt64ValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="ulong"/>.</summary>
    /// <param name="value">The <see cref="ulong"/> representation of the value.</param>
    public void Deconstruct(out ulong value) => value = Value;

    /// <summary>Determines whether <paramref name="left"/> is less than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(UInt64ValueObject<T> left, UInt64ValueObject<T> right)
        => left.Value < right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(UInt64ValueObject<T> left, UInt64ValueObject<T> right)
        => left.Value > right.Value;

    /// <summary>Determines whether <paramref name="left"/> is less than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(UInt64ValueObject<T> left, UInt64ValueObject<T> right)
        => left.Value <= right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(UInt64ValueObject<T> left, UInt64ValueObject<T> right)
        => left.Value >= right.Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="float"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record FloatValueObject<T> where T : FloatValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected FloatValueObject(float value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected float Value { get; init; }

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(FloatValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="float"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="float"/> representation of the value.</returns>
    public static implicit operator float(FloatValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="float"/>.</summary>
    /// <param name="value">The <see cref="float"/> representation of the value.</param>
    public void Deconstruct(out float value) => value = Value;

    /// <summary>Determines whether <paramref name="left"/> is less than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(FloatValueObject<T> left, FloatValueObject<T> right)
        => left.Value < right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(FloatValueObject<T> left, FloatValueObject<T> right)
        => left.Value > right.Value;

    /// <summary>Determines whether <paramref name="left"/> is less than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(FloatValueObject<T> left, FloatValueObject<T> right)
        => left.Value <= right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(FloatValueObject<T> left, FloatValueObject<T> right)
        => left.Value >= right.Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="double"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record DoubleValueObject<T> where T : DoubleValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected DoubleValueObject(double value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected double Value { get; init; }

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(DoubleValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="double"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="double"/> representation of the value.</returns>
    public static implicit operator double(DoubleValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="double"/>.</summary>
    /// <param name="value">The <see cref="double"/> representation of the value.</param>
    public void Deconstruct(out double value) => value = Value;

    /// <summary>Determines whether <paramref name="left"/> is less than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(DoubleValueObject<T> left, DoubleValueObject<T> right)
        => left.Value < right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(DoubleValueObject<T> left, DoubleValueObject<T> right)
        => left.Value > right.Value;

    /// <summary>Determines whether <paramref name="left"/> is less than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(DoubleValueObject<T> left, DoubleValueObject<T> right)
        => left.Value <= right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(DoubleValueObject<T> left, DoubleValueObject<T> right)
        => left.Value >= right.Value;
}

/// <summary>Base record for a strongly typed value object that wraps a <see cref="decimal"/> value.</summary>
/// <typeparam name="T">The concrete value object type.</typeparam>
[GenerateSerializer, Immutable]
public abstract record DecimalValueObject<T> where T : DecimalValueObject<T>
{
    /// <summary>Initializes a new instance with the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    protected DecimalValueObject(decimal value) => Value = value;

    /// <summary>Gets the wrapped value.</summary>
    [Id(0), JsonInclude] protected decimal Value { get; init; }

    /// <summary>Converts the value object to a <see cref="string"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="string"/> representation of the value.</returns>
    public static implicit operator string(DecimalValueObject<T> self) => self.ToString();

    /// <summary>Converts the value object to a <see cref="decimal"/>.</summary>
    /// <param name="self">The value object to convert.</param>
    /// <returns>The <see cref="decimal"/> representation of the value.</returns>
    public static implicit operator decimal(DecimalValueObject<T> self) => self.Value;

    /// <summary>Deconstructs the value object into a <see cref="string"/>.</summary>
    /// <param name="value">The <see cref="string"/> representation of the value.</param>
    public void Deconstruct(out string value) => value = ToString();

    /// <summary>Deconstructs the value object into a <see cref="decimal"/>.</summary>
    /// <param name="value">The <see cref="decimal"/> representation of the value.</param>
    public void Deconstruct(out decimal value) => value = Value;

    /// <summary>Determines whether <paramref name="left"/> is less than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(DecimalValueObject<T> left, DecimalValueObject<T> right)
        => left.Value < right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(DecimalValueObject<T> left, DecimalValueObject<T> right)
        => left.Value > right.Value;

    /// <summary>Determines whether <paramref name="left"/> is less than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(DecimalValueObject<T> left, DecimalValueObject<T> right)
        => left.Value <= right.Value;

    /// <summary>Determines whether <paramref name="left"/> is greater than or equal to <paramref name="right"/>.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(DecimalValueObject<T> left, DecimalValueObject<T> right)
        => left.Value >= right.Value;
}
