using Continuum.Orleans.Events;
using Continuum.Orleans.Streams;
using Continuum.Streaming.Orleans.KurrentDB.Extensions;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that checkpoint markers are mapped by every mapper, travel through the queue cache, and never reach a
///     custom stream id mapper.
/// </summary>
public class CheckpointMarkerTests
{
    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private sealed class MarkerSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => BinaryData.FromBytes([1]);

        public T Deserialize<T>(BinaryData input) => (T)(object)CheckpointMarker.Instance;
    }

    private sealed class TestAdapter(KurrentDBDataAdapterOptions options) : KurrentDBQueueDataAdapter(OrleansSerializer, options)
    {
        public IBatchContainer Build(KurrentDBMessage message) => GetBatchContainer(message);
    }

    private static KurrentDBDataAdapterOptions CreateOptions()
    {
        var typeMapper = new DefaultTypeMapAttributeMapper();
        typeMapper.RegisterKnownTypes(TypeMapKinds.DomainEvent);
        return new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            ProviderName = "test",
            TypeMapper = typeMapper,
            GrainStorageSerializer = new MarkerSerializer(),
            StreamIdMapper = name => throw new InvalidOperationException($"Custom mapper received {name}."),
        };
    }

    [Fact]
    public void Every_mapper_registers_the_marker_under_its_type_name()
    {
        var typeMapper = new DefaultTypeMapAttributeMapper();
        typeMapper.RegisterKnownTypes(TypeMapKinds.Command);

        Assert.Equal(CheckpointMarker.TypeName, typeMapper.GetTypeName(typeof(CheckpointMarker)));
    }

    [Fact]
    public void Self_hosted_checkpoint_events_use_the_marker_type_name()
    {
        var allStream = KurrentDBStreamedEventConversions.ToAllStreamCheckpointEvent(new global::KurrentDB.Client.Position(10, 10), "test");

        Assert.Equal(CheckpointMarker.TypeName, allStream.EventType);
        Assert.Same(CheckpointMarker.Instance, allStream.Event);
    }

    [Fact]
    public void Marker_carrier_is_positioned_on_the_reserved_stream()
    {
        var marker = KurrentDBCheckpointMarkerData.Create(CreateOptions());

        var carrier = KurrentDBStreamedEventFactory.CheckpointMarker(42, "test", marker);

        Assert.Equal(CheckpointMarker.TypeName, carrier.EventType);
        Assert.Equal(KurrentDBStreamedEventConversions.CheckpointStreamName.Value, carrier.StreamName.Value);
        Assert.Equal(42, (long)carrier.Cursor.SequenceNumber);
    }

    [Fact]
    public void Marker_bypasses_custom_stream_id_mapper()
    {
        var options = CreateOptions();
        var carrier = KurrentDBStreamedEventFactory.CheckpointMarker(42, "test", KurrentDBCheckpointMarkerData.Create(options));

        var streamId = new TestAdapter(options).GetStreamId(carrier);

        Assert.Equal(KurrentDBStreamedEventConversions.CheckpointStreamName.Value, streamId.GetKeyAsString());
    }

    [Fact]
    public void Marker_materializes_from_the_cache()
    {
        var options = CreateOptions();
        var marker = KurrentDBCheckpointMarkerData.Create(options);
        var message = new KurrentDBMessage("test", StreamId.Create("test", "$checkpoint"), 0, 42, Guid.NewGuid(), marker.EventType, marker.Data, KurrentDBStreamedEventConversions.CheckpointStreamName);

        var events = new TestAdapter(options).Build(message).GetEvents<IStreamedEvent<object>>().ToList();

        Assert.IsType<CheckpointMarker>(Assert.Single(events).Item1.Event);
    }
}
