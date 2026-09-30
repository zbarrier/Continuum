namespace Continuum.Serialization.Orleans;

/// <summary>
/// <see cref="BinaryData"/> that carries the CLR type to deserialize to, as resolved from a type mapper by event-sourcing storage.
/// </summary>
public sealed class BinaryDataWithType : BinaryData
{
    /// <summary>Initializes a new instance.</summary>
    /// <param name="data">The serialized payload.</param>
    /// <param name="type">The CLR type to deserialize the payload to.</param>
    public BinaryDataWithType(ReadOnlyMemory<byte> data, Type type) : base(data)
    {
        Type = type;
    }

    /// <summary>Gets the CLR type to deserialize the payload to.</summary>
    public Type Type { get; }
}
