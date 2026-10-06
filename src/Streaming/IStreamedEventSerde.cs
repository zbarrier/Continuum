namespace Continuum.Streaming;

public interface IStreamedEventSerde
{
    object? Deserialize(ReadOnlySpan<byte> data, Type type);
    byte[] Serialize(object? data);
}