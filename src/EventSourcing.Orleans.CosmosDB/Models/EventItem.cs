using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

using System.Runtime.Serialization;

namespace Continuum.EventSourcing.Orleans.CosmosDB;

public enum EventItemType
{
    [EnumMember(Value = "hdr")]
    Header = 1,
    [EnumMember(Value = "ss")]
    Snapshot = 2,
    [EnumMember(Value = "evt")]
    Event = 3,
}

public sealed class EventItem(string id, EventItemType type, string streamName)
{
    [JsonProperty("id")]
    public string Id { get; init; } = id;

    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public EventItemType Type { get; init; } = type;

    [JsonProperty("streamName")]
    public string StreamName { get; init; } = streamName;

    [JsonProperty("ver")]
    public ulong Version { get; set; }

    [JsonProperty("dataType")]
    public string DataType { get; set; } = default!;
    [JsonProperty("data")]
    public JToken Data { get; set; } = default!;

    [JsonProperty("meta")]
    public Dictionary<string, string> Metadata { get; set; } = default!;

    [JsonProperty("sort")]
    public decimal SortOrder => Version + GetOrderingFraction(Type);

    [JsonProperty("del")]
    public bool Deleted { get; set; } = false;

    [JsonProperty("subSeq")]
    public ulong SubSequenceNumber { get; set; }

    [JsonProperty("_etag")]
    public string ETag { get; set; } = default!;

    [JsonProperty("_ts")]
    public long Timestamp { get; set; } = default!;

    [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
    public int? TimeToLive { get; set; } = null;

    internal static decimal GetOrderingFraction(EventItemType type)
    {
        return type switch
        {
            EventItemType.Header => 0.3M,
            EventItemType.Snapshot => 0.2M,
            EventItemType.Event => 0.1M,
            _ => throw new NotSupportedException($"Event data item type '{type}' is not supported."),
        };
    }
}
