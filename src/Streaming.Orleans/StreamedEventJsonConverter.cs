using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Continuum.TypeMapping;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// System.Text.Json converter for <see cref="IStreamedEvent{T}"/> of <see cref="object"/> held in grain state.
/// </summary>
/// <remarks>
/// The payload is written with its runtime type and restored through <see cref="ITypeMapper"/> using
/// <see cref="IStreamedEvent{T}.EventType"/>, so the payload type must be mapped (for example with a domain event type
/// attribute). Metadata is written inline and restored as a <see cref="StreamedEventMetadata"/>.
/// </remarks>
/// <param name="typeMapper">The type mapper used to resolve payload types from event types.</param>
public sealed class StreamedEventJsonConverter(ITypeMapper typeMapper) : JsonConverter<IStreamedEvent<object>>
{
    private readonly ITypeMapper _typeMapper = typeMapper ?? throw new ArgumentNullException(nameof(typeMapper));

    /// <inheritdoc/>
    public override IStreamedEvent<object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var eventType = root.GetProperty("eventType").GetString()!;
        var payloadType = _typeMapper.GetType(eventType);
        var payload = root.GetProperty("event").Deserialize(options.GetTypeInfo(payloadType))
            ?? throw new JsonException($"Streamed event '{eventType}' has a null payload.");

        return new StreamedEvent<object>(
            NewId.FromSequentialGuid(root.GetProperty("eventId").GetGuid()),
            eventType,
            root.GetProperty("streamName").GetString()!,
            root.GetProperty("streamKey").GetString()!,
            root.GetProperty("topic").GetString()!,
            root.GetProperty("partitionId").GetString()!,
            root.GetProperty("streamVersion").GetUInt64(),
            root.GetProperty("streamPosition").GetUInt64(),
            BigInteger.Parse(root.GetProperty("sequenceNumber").GetString()!, CultureInfo.InvariantCulture),
            root.GetProperty("subSequenceNumber").GetUInt64(),
            root.GetProperty("timestamp").GetDateTime(),
            payload,
            ReadMetadata(root.GetProperty("metadata")));
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IStreamedEvent<object> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("eventId", value.EventId.ToSequentialGuid());
        writer.WriteString("eventType", value.EventType);
        writer.WriteString("streamName", value.StreamName);
        writer.WriteString("streamKey", value.StreamKey);
        writer.WriteString("topic", value.Topic);
        writer.WriteString("partitionId", value.PartitionId);
        writer.WriteNumber("streamVersion", value.StreamVersion);
        writer.WriteNumber("streamPosition", value.StreamPosition);
        writer.WriteString("sequenceNumber", value.SequenceNumber.ToString(CultureInfo.InvariantCulture));
        writer.WriteNumber("subSequenceNumber", value.SubSequenceNumber);
        writer.WriteString("timestamp", value.Timestamp);
        writer.WritePropertyName("event");
        JsonSerializer.Serialize(writer, value.Event, options.GetTypeInfo(value.Event.GetType()));
        writer.WritePropertyName("metadata");
        WriteMetadata(writer, value.Metadata);
        writer.WriteEndObject();
    }

    private static void WriteMetadata(Utf8JsonWriter writer, IStreamedEventMetadata metadata)
    {
        writer.WriteStartObject();
        writer.WriteStartObject("values");
        foreach (var (key, entry) in metadata)
        {
            writer.WriteString(key, entry);
        }
        writer.WriteEndObject();
        WriteIfNotNull(writer, "traceId", metadata.TraceId);
        WriteIfNotNull(writer, "spanId", metadata.SpanId);
        if (metadata.TransactionId is { } transactionId)
        {
            writer.WriteString("transactionId", transactionId.ToSequentialGuid());
        }
        WriteIfNotNull(writer, "transactionSize", metadata.TransactionSize);
        WriteIfNotNull(writer, "transactionPartitionSize", metadata.TransactionPartitionSize);
        WriteIfNotNull(writer, "transactionPartitionIndex", metadata.TransactionPartitionIndex);
        writer.WriteEndObject();
    }

    private static StreamedEventMetadata ReadMetadata(JsonElement element)
    {
        var values = new List<KeyValuePair<string, string?>>();
        if (element.TryGetProperty("values", out var valuesElement))
        {
            foreach (var property in valuesElement.EnumerateObject())
            {
                values.Add(new(property.Name, property.Value.GetString()));
            }
        }

        return StreamedEventMetadata.Create(
            values,
            element.TryGetProperty("traceId", out var traceId) ? traceId.GetString() : null,
            element.TryGetProperty("spanId", out var spanId) ? spanId.GetString() : null,
            element.TryGetProperty("transactionId", out var transactionId) ? NewId.FromSequentialGuid(transactionId.GetGuid()) : null,
            element.TryGetProperty("transactionSize", out var size) ? size.GetInt32() : null,
            element.TryGetProperty("transactionPartitionSize", out var partitionSize) ? partitionSize.GetInt32() : null,
            element.TryGetProperty("transactionPartitionIndex", out var partitionIndex) ? partitionIndex.GetInt32() : null);
    }

    private static void WriteIfNotNull(Utf8JsonWriter writer, string name, string? value)
    {
        if (value is not null)
        {
            writer.WriteString(name, value);
        }
    }

    private static void WriteIfNotNull(Utf8JsonWriter writer, string name, int? value)
    {
        if (value is { } number)
        {
            writer.WriteNumber(name, number);
        }
    }
}
