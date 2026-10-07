using System.Text;

using Continuum.Streaming.Orleans.KurrentDB;

using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.Tests;

/// <summary>
/// Verifies that a catch-up subscription skips events that have no payload, cannot be read, or read as null, rather
/// than passing them on to subscribers or letting the failure stop the subscription.
/// </summary>
public class KurrentDBCatchupEventDeserializerTests
{
    private sealed record KnownEvent(string Value);

    private sealed class FakeSerializer(Func<BinaryData, object?> deserialize) : IGrainStorageSerializer
    {
        public int Calls { get; private set; }

        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input)
        {
            Calls++;
            return (T)deserialize(input)!;
        }
    }

    private static readonly byte[] Payload = Encoding.UTF8.GetBytes("{}");

    private static bool TryDeserialize(FakeSerializer serializer, byte[] data, out object? deserializedEvent)
    {
        return KurrentDBCatchupEventDeserializer.TryDeserialize(serializer, NullLogger.Instance, "test-known-event", "test-1", data, typeof(KnownEvent), "test-subscription", out deserializedEvent);
    }

    [Fact]
    public void Returns_deserialized_event()
    {
        var serializer = new FakeSerializer(_ => new KnownEvent("a"));

        Assert.True(TryDeserialize(serializer, Payload, out var deserializedEvent));
        Assert.Equal(new KnownEvent("a"), deserializedEvent);
    }

    [Fact]
    public void Skips_empty_payload_without_deserializing()
    {
        var serializer = new FakeSerializer(_ => new KnownEvent("a"));

        Assert.False(TryDeserialize(serializer, [], out var deserializedEvent));
        Assert.Null(deserializedEvent);
        Assert.Equal(0, serializer.Calls);
    }

    [Fact]
    public void Skips_payload_that_cannot_be_deserialized()
    {
        var serializer = new FakeSerializer(_ => throw new FormatException("Corrupt."));

        Assert.False(TryDeserialize(serializer, Payload, out var deserializedEvent));
        Assert.Null(deserializedEvent);
    }

    [Fact]
    public void Skips_payload_that_deserializes_to_null()
    {
        var serializer = new FakeSerializer(_ => null);

        Assert.False(TryDeserialize(serializer, Payload, out var deserializedEvent));
        Assert.Null(deserializedEvent);
    }
}
