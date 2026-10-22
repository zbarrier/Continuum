using System.Text;
using System.Text.Json;

using Continuum.Serialization.Orleans;

using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Storage;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Tests;

/// <summary>
/// Verifies that a catch-up subscription skips changes that have no payload, cannot be read or read as null, rather
/// than passing them on to subscribers or letting the failure stop the subscription.
/// </summary>
public class CosmosDBCatchupEventDeserializerTests
{
    private sealed record KnownEvent(string Value);

    private sealed class FakeSerializer(Func<BinaryData, object?> deserialize) : IGrainStorageSerializer
    {
        public int Calls { get; private set; }

        public BinaryData? LastInput { get; private set; }

        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input)
        {
            Calls++;
            LastInput = input;
            return (T)deserialize(input)!;
        }
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static bool TryDeserialize(FakeSerializer serializer, JsonElement data, out object? deserializedEvent)
    {
        var change = new ChangeFeedEventItem
        {
            Id = Guid.NewGuid().ToString(),
            Type = EventItemType.Event,
            StreamName = "test/1",
            DataType = "test-known-event",
            Data = data,
        };
        return CosmosDBCatchupEventDeserializer.TryDeserialize(change, typeof(KnownEvent), serializer, NullLogger.Instance, out deserializedEvent);
    }

    [Fact]
    public void Returns_deserialized_event()
    {
        var serializer = new FakeSerializer(_ => new KnownEvent("a"));

        Assert.True(TryDeserialize(serializer, Json("""{"Value":"a"}"""), out var deserializedEvent));
        Assert.Equal(new KnownEvent("a"), deserializedEvent);
    }

    [Fact]
    public void Passes_the_payload_json_and_target_type_to_the_serializer()
    {
        var serializer = new FakeSerializer(_ => new KnownEvent("a"));

        TryDeserialize(serializer, Json("""{"Value":"a"}"""), out _);

        var input = Assert.IsType<BinaryDataWithType>(serializer.LastInput);
        Assert.Equal(typeof(KnownEvent), input.Type);
        Assert.Equal("""{"Value":"a"}""", Encoding.UTF8.GetString(input.ToArray()));
    }

    [Fact]
    public void Skips_missing_payload_without_deserializing()
    {
        var serializer = new FakeSerializer(_ => new KnownEvent("a"));

        Assert.False(TryDeserialize(serializer, default, out var deserializedEvent));
        Assert.Null(deserializedEvent);
        Assert.Equal(0, serializer.Calls);
    }

    [Fact]
    public void Skips_null_payload_without_deserializing()
    {
        var serializer = new FakeSerializer(_ => new KnownEvent("a"));

        Assert.False(TryDeserialize(serializer, Json("null"), out var deserializedEvent));
        Assert.Null(deserializedEvent);
        Assert.Equal(0, serializer.Calls);
    }

    [Fact]
    public void Skips_payload_that_cannot_be_deserialized()
    {
        var serializer = new FakeSerializer(_ => throw new FormatException("Corrupt."));

        Assert.False(TryDeserialize(serializer, Json("[1,2,3]"), out var deserializedEvent));
        Assert.Null(deserializedEvent);
    }

    [Fact]
    public void Skips_payload_that_deserializes_to_null()
    {
        var serializer = new FakeSerializer(_ => null);

        Assert.False(TryDeserialize(serializer, Json("{}"), out var deserializedEvent));
        Assert.Null(deserializedEvent);
    }
}
