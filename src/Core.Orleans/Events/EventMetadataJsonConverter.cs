using Continuum.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Continuum.Orleans.Events;

/// <summary>
///     Reads and writes <see cref="EventMetadata" /> for System.Text.Json.
/// </summary>
/// <remarks>
///     <see cref="EventMetadata" /> is a read-only dictionary with a private constructor, so it cannot be rebuilt by
///     the default object converter. Rebuilding goes through
///     <see cref="EventMetadata.Create(IEnumerable{KeyValuePair{string, string}}, string, string, TransactionInfo?)" />
///     so the same filtering rules apply as for every other source.
/// </remarks>
public sealed class EventMetadataJsonConverter : JsonConverter<EventMetadata>
{
    private const string ValuesName = "values";
    private const string TraceIdName = "traceId";
    private const string SpanIdName = "spanId";
    private const string TransactionIdName = "transactionId";
    private const string TransactionSizeName = "transactionSize";
    private const string TransactionPartitionSizeName = "transactionPartitionSize";
    private const string TransactionPartitionIndexName = "transactionPartitionIndex";

    /// <inheritdoc />
    public override EventMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadMetadata(ref reader);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EventMetadata value, JsonSerializerOptions options)
    {
        WriteMetadata(writer, value);
    }

    /// <summary>
    ///     Reads metadata written by <see cref="WriteMetadata" />.
    /// </summary>
    /// <param name="reader">The reader, positioned on the start of the metadata object.</param>
    /// <returns>The metadata read.</returns>
    /// <exception cref="JsonException">The value is not a metadata object.</exception>
    public static EventMetadata ReadMetadata(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected the start of an event metadata object.");
        }

        var values = new List<KeyValuePair<string, string?>>();
        string? traceId = null;
        string? spanId = null;
        Guid? transactionId = null;
        int? transactionSize = null;
        int? transactionPartitionSize = null;
        int? transactionPartitionIndex = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var name = reader.GetString();
            reader.Read();
            switch (name)
            {
                case ValuesName:
                    ReadValues(ref reader, values);
                    break;
                case TraceIdName:
                    traceId = reader.GetString();
                    break;
                case SpanIdName:
                    spanId = reader.GetString();
                    break;
                case TransactionIdName:
                    transactionId = reader.TokenType == JsonTokenType.Null ? null : reader.GetGuid();
                    break;
                case TransactionSizeName:
                    transactionSize = ReadInt32(ref reader);
                    break;
                case TransactionPartitionSizeName:
                    transactionPartitionSize = ReadInt32(ref reader);
                    break;
                case TransactionPartitionIndexName:
                    transactionPartitionIndex = ReadInt32(ref reader);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        TransactionInfo? transaction = null;
        if (transactionId is { } id && transactionSize is { } size && transactionPartitionSize is { } partitionSize &&
            transactionPartitionIndex is { } partitionIndex)
        {
            var candidate = new TransactionInfo(NewId.FromSequentialGuid(id), size, partitionSize, partitionIndex);
            if (!candidate.IsValid)
            {
                throw new JsonException(
                    $"Event metadata has an invalid transaction grouping: size {size}, partition size {partitionSize}, partition index {partitionIndex}.");
            }
            transaction = candidate;
        }
        else if (transactionId is not null || transactionSize is not null || transactionPartitionSize is not null ||
            transactionPartitionIndex is not null)
        {
            throw new JsonException(
                $"Event metadata has an incomplete transaction; {TransactionIdName}, {TransactionSizeName}, {TransactionPartitionSizeName} and {TransactionPartitionIndexName} must all be present or all be absent.");
        }

        return EventMetadata.Create(values, traceId, spanId, transaction);
    }

    /// <summary>
    ///     Writes metadata as a JSON object.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="metadata">The metadata to write.</param>
    public static void WriteMetadata(Utf8JsonWriter writer, IEventMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(metadata);

        writer.WriteStartObject();
        writer.WriteStartObject(ValuesName);
        foreach (var (key, entry) in metadata)
        {
            writer.WriteString(key, entry);
        }
        writer.WriteEndObject();
        if (metadata.TraceId is { } traceId)
        {
            writer.WriteString(TraceIdName, traceId);
        }
        if (metadata.SpanId is { } spanId)
        {
            writer.WriteString(SpanIdName, spanId);
        }
        if (metadata.TransactionId is { } transactionId)
        {
            writer.WriteString(TransactionIdName, transactionId.ToSequentialGuid());
        }
        WriteIfNotNull(writer, TransactionSizeName, metadata.TransactionSize);
        WriteIfNotNull(writer, TransactionPartitionSizeName, metadata.TransactionPartitionSize);
        WriteIfNotNull(writer, TransactionPartitionIndexName, metadata.TransactionPartitionIndex);
        writer.WriteEndObject();
    }

    private static void ReadValues(ref Utf8JsonReader reader, List<KeyValuePair<string, string?>> values)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return;
        }
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected event metadata values to be an object.");
        }
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var key = reader.GetString()!;
            reader.Read();
            values.Add(new(key, reader.TokenType == JsonTokenType.String ? reader.GetString() : null));
            if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
            {
                reader.Skip();
            }
        }
    }

    private static int? ReadInt32(ref Utf8JsonReader reader)
    {
        return reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();
    }

    private static void WriteIfNotNull(Utf8JsonWriter writer, string name, int? value)
    {
        if (value is { } number)
        {
            writer.WriteNumber(name, number);
        }
    }
}

/// <summary>
///     Reads and writes <see cref="IEventMetadata" /> for System.Text.Json, restoring it as <see cref="EventMetadata" />.
/// </summary>
/// <remarks>
///     Apply to members declared as <see cref="IEventMetadata" />, which the default converters cannot rebuild.
/// </remarks>
public sealed class IEventMetadataJsonConverter : JsonConverter<IEventMetadata>
{
    /// <inheritdoc />
    public override IEventMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return EventMetadataJsonConverter.ReadMetadata(ref reader);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IEventMetadata value, JsonSerializerOptions options)
    {
        EventMetadataJsonConverter.WriteMetadata(writer, value);
    }
}
