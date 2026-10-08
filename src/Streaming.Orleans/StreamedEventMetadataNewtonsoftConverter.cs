using Newtonsoft.Json;

namespace Continuum.Streaming.Orleans;

/// <summary>
///     Reads and writes <see cref="StreamedEventMetadata" /> for Newtonsoft.Json, which Orleans uses for its default
///     grain storage serializer.
/// </summary>
/// <remarks>
///     <see cref="StreamedEventMetadata" /> is a read-only dictionary with a private constructor, so Newtonsoft can
///     write it but cannot rebuild it, which fails grain activation for any state that holds a streamed event.
///     Rebuilding goes through <see cref="StreamedEventMetadata.Create(IEnumerable{KeyValuePair{string, string}}, string, string, NewId?, int?, int?, int?)" />
///     so the same filtering rules apply as for every other source.
/// </remarks>
internal sealed class StreamedEventMetadataNewtonsoftConverter : JsonConverter<IStreamedEventMetadata>
{
    private const string ValuesName = "values";
    private const string TraceIdName = "traceId";
    private const string SpanIdName = "spanId";
    private const string TransactionIdName = "transactionId";
    private const string TransactionSizeName = "transactionSize";
    private const string TransactionPartitionSizeName = "transactionPartitionSize";
    private const string TransactionPartitionIndexName = "transactionPartitionIndex";

    public override void WriteJson(JsonWriter writer, IStreamedEventMetadata? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();

        writer.WritePropertyName(ValuesName);
        writer.WriteStartObject();
        foreach (var (key, entry) in value)
        {
            writer.WritePropertyName(key);
            writer.WriteValue(entry);
        }
        writer.WriteEndObject();

        writer.WritePropertyName(TraceIdName);
        writer.WriteValue(value.TraceId);
        writer.WritePropertyName(SpanIdName);
        writer.WriteValue(value.SpanId);
        writer.WritePropertyName(TransactionIdName);
        writer.WriteValue(value.TransactionId?.ToString());
        writer.WritePropertyName(TransactionSizeName);
        writer.WriteValue(value.TransactionSize);
        writer.WritePropertyName(TransactionPartitionSizeName);
        writer.WriteValue(value.TransactionPartitionSize);
        writer.WritePropertyName(TransactionPartitionIndexName);
        writer.WriteValue(value.TransactionPartitionIndex);

        writer.WriteEndObject();
    }

    public override IStreamedEventMetadata? ReadJson(JsonReader reader, Type objectType, IStreamedEventMetadata? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonToken.StartObject)
        {
            throw new JsonSerializationException($"Expected an object for {nameof(StreamedEventMetadata)} but found {reader.TokenType}.");
        }

        var values = new Dictionary<string, string?>(StringComparer.Ordinal);
        string? traceId = null;
        string? spanId = null;
        NewId? transactionId = null;
        int? transactionSize = null;
        int? transactionPartitionSize = null;
        int? transactionPartitionIndex = null;

        while (reader.Read() && reader.TokenType != JsonToken.EndObject)
        {
            var name = (string)reader.Value!;
            switch (name)
            {
                case ValuesName:
                    reader.Read();
                    if (reader.TokenType == JsonToken.Null)
                    {
                        break;
                    }
                    while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                    {
                        var key = (string)reader.Value!;
                        values[key] = reader.ReadAsString();
                    }
                    break;
                case TraceIdName:
                    traceId = reader.ReadAsString();
                    break;
                case SpanIdName:
                    spanId = reader.ReadAsString();
                    break;
                case TransactionIdName:
                    var text = reader.ReadAsString();
                    transactionId = text is null ? null : new NewId(text);
                    break;
                case TransactionSizeName:
                    transactionSize = reader.ReadAsInt32();
                    break;
                case TransactionPartitionSizeName:
                    transactionPartitionSize = reader.ReadAsInt32();
                    break;
                case TransactionPartitionIndexName:
                    transactionPartitionIndex = reader.ReadAsInt32();
                    break;
                default:
                    reader.Read();
                    reader.Skip();
                    break;
            }
        }

        return StreamedEventMetadata.Create(values, traceId, spanId, transactionId, transactionSize, transactionPartitionSize,
            transactionPartitionIndex);
    }
}
