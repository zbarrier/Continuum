using Continuum.Events;
using Continuum.Orleans.Events;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Continuum.TypeMapping;
using Continuum.Orleans.Streams;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// System.Text.Json converter for <see cref="IStreamedEvent{T}"/> of <see cref="object"/> held in grain state.
/// </summary>
/// <remarks>
/// The payload is written with its runtime type and restored through <see cref="ITypeMapper"/> using
/// <see cref="IStreamedEvent{T}.EventType"/>, so the payload type must be mapped (for example with a domain event type
/// attribute). Metadata is written inline and restored as a <see cref="EventMetadata"/>.
/// </remarks>
/// <param name="typeMapper">The type mapper used to resolve payload types from event types.</param>
public sealed class StreamedEventJsonConverter(ITypeMapper typeMapper) : JsonConverter<IStreamedEvent<object>>
{
    private readonly ITypeMapper _typeMapper = typeMapper ?? throw new ArgumentNullException(nameof(typeMapper));

    /// <inheritdoc/>
    public override IStreamedEvent<object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected the start of a streamed event object.");
        }

        Guid? eventId = null;
        string? eventType = null;
        StreamName? streamName = null;
        ulong? streamVersion = null;
        StreamedEventCursor? cursor = null;
        DateTime? timestamp = null;
        object? payload = null;
        var hasPayload = false;
        JsonElement? deferredPayload = null;
        EventMetadata? metadata = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var name = reader.GetString();
            reader.Read();
            switch (name)
            {
                case "eventId":
                    eventId = reader.GetGuid();
                    break;
                case "eventType":
                    eventType = reader.GetString();
                    break;
                case "streamName":
                    streamName = ReadStreamName(ref reader);
                    break;
                case "streamVersion":
                    streamVersion = reader.GetUInt64();
                    break;
                case "cursor":
                    cursor = ReadCursor(ref reader);
                    break;
                case "timestamp":
                    timestamp = reader.GetDateTime();
                    break;
                case "event":
                    hasPayload = true;
                    if (eventType is null)
                    {
                        // The payload type is only known from eventType; buffer just the payload if it came first.
                        deferredPayload = JsonElement.ParseValue(ref reader);
                    }
                    else
                    {
                        payload = JsonSerializer.Deserialize(ref reader, options.GetTypeInfo(_typeMapper.GetType(eventType)));
                    }
                    break;
                case "metadata":
                    metadata = EventMetadataJsonConverter.ReadMetadata(ref reader);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        var resolvedEventType = Required(eventType, "eventType");
        if (!hasPayload)
        {
            throw new JsonException("Streamed event is missing required property 'event'.");
        }
        if (deferredPayload is { } element)
        {
            payload = element.Deserialize(options.GetTypeInfo(_typeMapper.GetType(resolvedEventType)));
        }

        return new StreamedEvent<object>(
            Required(eventId, "eventId"),
            resolvedEventType,
            Required(streamName, "streamName"),
            Required(streamVersion, "streamVersion"),
            Required(cursor, "cursor"),
            Required(timestamp, "timestamp"),
            payload ?? throw new JsonException($"Streamed event '{resolvedEventType}' has a null payload."),
            metadata ?? throw new JsonException("Streamed event is missing required property 'metadata'."));
    }

    private static StreamName ReadStreamName(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected 'streamName' to be an object.");
        }

        string? value = null;
        string? category = null;
        string? tenantId = null;
        string? key = null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var name = reader.GetString();
            reader.Read();
            switch (name)
            {
                case "value":
                    value = reader.GetString();
                    break;
                case "category":
                    category = reader.GetString();
                    break;
                case "tenantId":
                    tenantId = reader.GetString();
                    break;
                case "key":
                    key = reader.GetString();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return new StreamName(Required(value, "streamName.value"), Required(category, "streamName.category"), tenantId, Required(key, "streamName.key"));
    }

    private static StreamedEventCursor ReadCursor(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected 'cursor' to be an object.");
        }

        string? topic = null;
        string? partition = null;
        BigInteger? sequenceNumber = null;
        ulong? subSequenceNumber = null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var name = reader.GetString();
            reader.Read();
            switch (name)
            {
                case "topic":
                    topic = reader.GetString();
                    break;
                case "partition":
                    partition = reader.GetString();
                    break;
                case "sequenceNumber":
                    sequenceNumber = BigInteger.Parse(Required(reader.GetString(), "cursor.sequenceNumber"), CultureInfo.InvariantCulture);
                    break;
                case "subSequenceNumber":
                    subSequenceNumber = reader.GetUInt64();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return new StreamedEventCursor(
            Required(topic, "cursor.topic"),
            Required(partition, "cursor.partition"),
            Required(sequenceNumber, "cursor.sequenceNumber"),
            Required(subSequenceNumber, "cursor.subSequenceNumber"));
    }

    private static T Required<T>(T? value, string name) where T : class =>
        value ?? throw new JsonException($"Streamed event is missing required property '{name}'.");

    private static T Required<T>(T? value, string name) where T : struct =>
        value ?? throw new JsonException($"Streamed event is missing required property '{name}'.");

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IStreamedEvent<object> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("eventId", value.EventId);
        writer.WriteString("eventType", value.EventType);
        writer.WriteStartObject("streamName");
        writer.WriteString("value", value.StreamName.Value);
        writer.WriteString("category", value.StreamName.Category);
        if (value.StreamName.HasTenant)
        {
            writer.WriteString("tenantId", value.StreamName.TenantId);
        }
        writer.WriteString("key", value.StreamName.Key);
        writer.WriteEndObject();
        writer.WriteNumber("streamVersion", value.StreamVersion);
        writer.WriteStartObject("cursor");
        writer.WriteString("topic", value.Cursor.Topic);
        writer.WriteString("partition", value.Cursor.Partition);
        writer.WriteString("sequenceNumber", value.Cursor.SequenceNumber.ToString(CultureInfo.InvariantCulture));
        writer.WriteNumber("subSequenceNumber", value.Cursor.SubSequenceNumber);
        writer.WriteEndObject();
        writer.WriteString("timestamp", value.Timestamp);
        writer.WritePropertyName("event");
        JsonSerializer.Serialize(writer, value.Event, options.GetTypeInfo(value.Event.GetType()));
        writer.WritePropertyName("metadata");
        EventMetadataJsonConverter.WriteMetadata(writer, value.Metadata);
        writer.WriteEndObject();
    }
}
