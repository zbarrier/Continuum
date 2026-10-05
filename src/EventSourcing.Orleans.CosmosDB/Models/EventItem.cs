using System.Text.Json;
using System.Text.Json.Serialization;

namespace Continuum.EventSourcing.Orleans.CosmosDB;

/// <summary>
///     The kind of item stored in a stream partition.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<EventItemType>))]
public enum EventItemType
{
    /// <summary>The stream header holding the current version.</summary>
    [JsonStringEnumMemberName("hdr")]
    Header = 1,

    /// <summary>A snapshot item.</summary>
    [JsonStringEnumMemberName("ss")]
    Snapshot = 2,

    /// <summary>An event item.</summary>
    [JsonStringEnumMemberName("evt")]
    Event = 3,
}

/// <summary>
///     A Cosmos DB item belonging to a stream partition.
/// </summary>
public sealed class EventItem
{
    /// <summary>The item id, unique within the stream partition.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>The item kind.</summary>
    [JsonPropertyName("type")]
    public required EventItemType Type { get; init; }

    /// <summary>The stream name, which is also the partition key.</summary>
    [JsonPropertyName("streamName")]
    public required string StreamName { get; init; }

    /// <summary>For events, the version the event produces. For the header, the current stream version.</summary>
    [JsonPropertyName("ver")]
    public ulong Version { get; set; }

    /// <summary>The mapped type name of <see cref="Data"/>.</summary>
    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = string.Empty;

    /// <summary>The serialized payload.</summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }

    /// <summary>Item metadata.</summary>
    [JsonPropertyName("meta")]
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>Whether the stream is deleted (header only).</summary>
    [JsonPropertyName("del")]
    public bool Deleted { get; set; }

    /// <summary>The position of the event within the append that wrote it.</summary>
    [JsonPropertyName("subSeq")]
    public ulong SubSequenceNumber { get; set; }

    /// <summary>The Cosmos DB ETag; read only.</summary>
    [JsonPropertyName("_etag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ETag { get; set; }

    /// <summary>The Cosmos DB last-modified timestamp; read only.</summary>
    [JsonPropertyName("_ts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Timestamp { get; set; }

    /// <summary>Optional item time-to-live in seconds.</summary>
    [JsonPropertyName("ttl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TimeToLive { get; set; }
}

/// <summary>
///     The payload stored in the stream header item.
/// </summary>
public sealed class StreamHeader
{
    /// <summary>Whether the stream is archived.</summary>
    [JsonPropertyName("arch")]
    public bool Archived { get; set; }

    /// <summary>Whether the stream is deleted.</summary>
    [JsonPropertyName("del")]
    public bool Deleted { get; set; }

    /// <summary>Stream metadata.</summary>
    [JsonPropertyName("meta")]
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
///     The envelope of a Cosmos DB query stream response.
/// </summary>
internal sealed class EventItemQueryResponse
{
    [JsonPropertyName("Documents")]
    public List<EventItem> Documents { get; set; } = new();
}

[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(EventItem))]
[JsonSerializable(typeof(StreamHeader))]
[JsonSerializable(typeof(EventItemQueryResponse))]
internal sealed partial class CosmosDBJsonContext : JsonSerializerContext
{
}
