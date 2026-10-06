using System.Text.Json;

using Continuum.Converters.Json;

namespace Continuum.Streaming;

public sealed class SystemTextJsonStreamEventSerde : IStreamedEventSerde
{
    public static readonly SystemTextJsonStreamEventSerde Instance = new(new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new NewIdConverter()
        }
    });

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public SystemTextJsonStreamEventSerde(JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public object? Deserialize(ReadOnlySpan<byte> data, Type type)
    {
        var deserializedData = JsonSerializer.Deserialize(data, type, _jsonSerializerOptions);
        return deserializedData;
    }

    public T? Deserialize<T>(ReadOnlySpan<byte> data)
    {
        var deserializedData = JsonSerializer.Deserialize<T>(data, _jsonSerializerOptions);
        return deserializedData;
    }

    public byte[] Serialize(object? data)
    {
        var serializedData = JsonSerializer.SerializeToUtf8Bytes(data, _jsonSerializerOptions);
        return serializedData;
    }
}
