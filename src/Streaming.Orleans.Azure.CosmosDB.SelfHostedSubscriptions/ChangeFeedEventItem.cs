using System.Text.Json;
using System.Text.Json.Serialization;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions;

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
///     An item received from the Cosmos DB change feed of a container written by the Cosmos DB log-consistency provider.
/// </summary>
public sealed class ChangeFeedEventItem
{
    /// <summary>The item id, unique within the stream partition.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>The item kind.</summary>
    [JsonPropertyName("type")]
    public EventItemType Type { get; init; }

    /// <summary>The stream name, which is also the partition key.</summary>
    [JsonPropertyName("streamName")]
    public required string StreamName { get; init; }

    /// <summary>For events, the version the event produces. For the header, the current stream version.</summary>
    [JsonPropertyName("ver")]
    public ulong Version { get; init; }

    /// <summary>The mapped type name of <see cref="Data"/>.</summary>
    [JsonPropertyName("dataType")]
    public string DataType { get; init; } = string.Empty;

    /// <summary>The serialized payload.</summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; init; }

    /// <summary>Item metadata.</summary>
    [JsonPropertyName("meta")]
    public Dictionary<string, string>? Metadata { get; init; }

    /// <summary>Whether the stream is deleted (header only).</summary>
    [JsonPropertyName("del")]
    public bool Deleted { get; init; }

    /// <summary>The position of the event within the append that wrote it.</summary>
    [JsonPropertyName("subSeq")]
    public ulong SubSequenceNumber { get; init; }

    /// <summary>The Cosmos DB ETag.</summary>
    [JsonPropertyName("_etag")]
    public string? ETag { get; init; }

    /// <summary>The Cosmos DB last-modified timestamp, in Unix seconds.</summary>
    [JsonPropertyName("_ts")]
    public long Timestamp { get; init; }

    /// <summary>The item time-to-live in seconds, if set.</summary>
    [JsonPropertyName("ttl")]
    public int? TimeToLive { get; init; }

    /// <summary>The Cosmos DB logical sequence number of the change.</summary>
    [JsonPropertyName("_lsn")]
    public long LogicalSequenceNumber { get; init; }
}

/// <summary>
///     The envelope of a Cosmos DB change feed stream response.
/// </summary>
internal sealed class ChangeFeedResponse
{
    [JsonPropertyName("Documents")]
    public List<ChangeFeedEventItem> Documents { get; set; } = new();
}

[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(ChangeFeedResponse))]
internal sealed partial class ChangeFeedJsonContext : JsonSerializerContext
{
}
