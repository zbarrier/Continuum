using Microsoft.Extensions.Logging.Abstractions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.Tests;

/// <summary>
/// Verifies that a catch-up subscription skips changes that cannot be read or read as null, rather than passing them
/// on to subscribers or letting the failure stop the subscription.
/// </summary>
public class CosmosDBCatchupEventDeserializerTests
{
    private sealed record KnownEvent(string Value);

    private static bool TryDeserialize(JToken data, out object? deserializedEvent)
    {
        var change = new ChangeFeedEventItem
        {
            Id = Guid.NewGuid().ToString(),
            Type = EventItemType.Event,
            StreamName = "test-1",
            DataType = "test-known-event",
            Data = data,
        };
        return CosmosDBCatchupEventDeserializer.TryDeserialize(change, typeof(KnownEvent), JsonSerializer.CreateDefault(), NullLogger.Instance, out deserializedEvent);
    }

    [Fact]
    public void Returns_deserialized_event()
    {
        Assert.True(TryDeserialize(JObject.FromObject(new { Value = "a" }), out var deserializedEvent));
        Assert.Equal(new KnownEvent("a"), deserializedEvent);
    }

    [Fact]
    public void Skips_data_that_cannot_be_deserialized()
    {
        Assert.False(TryDeserialize(new JArray(1, 2, 3), out var deserializedEvent));
        Assert.Null(deserializedEvent);
    }

    [Fact]
    public void Skips_data_that_deserializes_to_null()
    {
        Assert.False(TryDeserialize(JValue.CreateNull(), out var deserializedEvent));
        Assert.Null(deserializedEvent);
    }
}

