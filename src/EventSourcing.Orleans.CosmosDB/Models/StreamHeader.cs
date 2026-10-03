using Newtonsoft.Json;

namespace Continuum.EventSourcing.Orleans.CosmosDB;

public sealed class StreamHeader(bool archived, bool deleted, Dictionary<string, string> metadata)
{
    [JsonProperty("arch")]
    public bool Archived { get; set; } = archived;

    [JsonProperty("del")]
    public bool Deleted { get; set; } = deleted;

    [JsonProperty("meta")]
    public Dictionary<string, string> Metadata { get; set; } = metadata;
}
