using Continuum.Orleans.Events;

using Orleans.Configuration;
using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     The serialized form of <see cref="Continuum.Orleans.Events.CheckpointMarker" /> for a stream provider.
/// </summary>
/// <remarks>
///     The marker is serialized once with the provider's configured serializer and type mapper, so it is read back
///     through the same path as every other cached event.
/// </remarks>
internal sealed class KurrentDBCheckpointMarkerData
{
    private KurrentDBCheckpointMarkerData(string eventType, byte[] data)
    {
        EventType = eventType;
        Data = data;
    }

    /// <summary>
    ///     The mapped type name of the marker.
    /// </summary>
    public string EventType { get; }

    /// <summary>
    ///     The serialized marker.
    /// </summary>
    public byte[] Data { get; }

    /// <summary>
    ///     Serializes the marker using the provider's data adapter options.
    /// </summary>
    /// <param name="options">The data adapter options.</param>
    public static KurrentDBCheckpointMarkerData Create(KurrentDBDataAdapterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        var eventType = options.TypeMapper.GetTypeName(typeof(CheckpointMarker));
        var data = options.GrainStorageSerializer.Serialize(CheckpointMarker.Instance).ToArray();
        return new KurrentDBCheckpointMarkerData(eventType, data);
    }
}
