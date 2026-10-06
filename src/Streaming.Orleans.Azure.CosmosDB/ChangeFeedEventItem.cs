using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB;

public enum EventItemType
{
    [EnumMember(Value = "hdr")]
    Header = 1,
    [EnumMember(Value = "ss")]
    Snapshot = 2,
    [EnumMember(Value = "evt")]
    Event = 3,
}

public sealed class ChangeFeedEventItem
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public EventItemType Type { get; set; }

    [JsonProperty("streamName")]
    public string StreamName { get; set; }

    [JsonProperty("ver")]
    public ulong Version { get; set; }

    [JsonProperty("dataType")]
    public string DataType { get; set; }
    [JsonProperty("data")]
    public JToken Data { get; set; }

    [JsonProperty("meta")]
    public Dictionary<string, string> Metadata { get; set; }

    [JsonProperty("del")]
    public bool Deleted { get; set; }

    [JsonProperty("_etag")]
    public string ETag { get; set; }

    [JsonProperty("_ts")]
    public long Timestamp { get; set; }

    //[JsonProperty("crts")]
    //public long ConflictResolvedTimestamp { get; set; }

    [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
    public int? TimeToLive { get; set; }

    [JsonProperty("_lsn")]
    public long LogicalSequenceNumber { get; set; }

    [JsonProperty("subSeq")]
    public ulong SubSequenceNumber { get; set; }
}
