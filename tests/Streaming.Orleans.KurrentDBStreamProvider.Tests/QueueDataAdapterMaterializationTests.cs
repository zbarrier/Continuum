using System.Text;

using Continuum.Streaming;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Configuration;
using Orleans.Providers.Streams.KurrentDB;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

/// <summary>
///     Verifies that the queue data adapters never hand a subscriber an event that has no payload, cannot be read, or
///     reads as null, and that unknown event types still follow the configured behavior.
/// </summary>
public class QueueDataAdapterMaterializationTests
{
    private const string KnownType = "test-known-event";

    private sealed record KnownEvent(string Value);

    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer(Func<BinaryData, object?> deserialize) : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => (T)deserialize(input)!;
    }

    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    public static TheoryData<bool> Versions => new() { false, true };

    private static IEnumerable<Tuple<IStreamedEvent<object>, StreamSequenceToken>> Materialize(bool v2, string eventType, byte[] data, Func<BinaryData, object?> deserialize, KurrentDBUnknownEventTypeBehavior unknownBehavior = KurrentDBUnknownEventTypeBehavior.Halt)
    {
        var typeMapper = new FakeTypeMapper();
        typeMapper.AddType(typeof(KnownEvent), KnownType);
        var options = new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            TypeMapper = typeMapper,
            GrainStorageSerializer = new FakeSerializer(deserialize),
            UnknownEventTypeBehavior = unknownBehavior,
        };
        var message = new KurrentDBMessage(StreamId.Create("test", "1"), "0", 0, 1, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid().ToString(), eventType, data, "test-1");
        IBatchContainer container = v2
            ? new TestAdapterV2(options).Build(message)
            : new TestAdapter(options).Build(message);
        return container.GetEvents<IStreamedEvent<object>>().ToList();
    }

    private sealed class TestAdapter(KurrentDBDataAdapterOptions options) : KurrentDBQueueDataAdapter(OrleansSerializer, options)
    {
        public IBatchContainer Build(KurrentDBMessage message) => GetBatchContainer(message);
    }

    private sealed class TestAdapterV2(KurrentDBDataAdapterOptions options) : KurrentDBQueueDataAdapterV2(OrleansSerializer, options)
    {
        public IBatchContainer Build(KurrentDBMessage message) => GetBatchContainer(message);
    }

    private static readonly byte[] Payload = Encoding.UTF8.GetBytes("{}");

    [Theory]
    [MemberData(nameof(Versions))]
    public void Delivers_resolved_event(bool v2)
    {
        var events = Materialize(v2, KnownType, Payload, _ => new KnownEvent("a"));

        Assert.Equal(new KnownEvent("a"), Assert.Single(events).Item1.Event);
    }

    [Theory]
    [MemberData(nameof(Versions))]
    public void Skips_empty_payload(bool v2)
    {
        var events = Materialize(v2, KnownType, [], _ => throw new InvalidOperationException("Should not be called."));

        Assert.Empty(events);
    }

    [Theory]
    [MemberData(nameof(Versions))]
    public void Skips_payload_that_cannot_be_deserialized(bool v2)
    {
        var events = Materialize(v2, KnownType, Payload, _ => throw new FormatException("Corrupt."));

        Assert.Empty(events);
    }

    [Theory]
    [MemberData(nameof(Versions))]
    public void Skips_payload_that_deserializes_to_null(bool v2)
    {
        var events = Materialize(v2, KnownType, Payload, _ => null);

        Assert.Empty(events);
    }

    [Theory]
    [MemberData(nameof(Versions))]
    public void Skips_unknown_type_when_configured_to_skip(bool v2)
    {
        var events = Materialize(v2, "unknown-type", Payload, _ => new KnownEvent("a"), KurrentDBUnknownEventTypeBehavior.Skip);

        Assert.Empty(events);
    }

    [Theory]
    [MemberData(nameof(Versions))]
    public void Faults_unknown_type_when_configured_to_halt(bool v2)
    {
        Assert.Throws<InvalidOperationException>(() => Materialize(v2, "unknown-type", Payload, _ => new KnownEvent("a")));
    }
}
