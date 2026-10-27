using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB;

/// <summary>
///     Stores subscription checkpoints as events in KurrentDB.
/// </summary>
public sealed class KurrentDBCheckpointStore : ICheckpointStore<ulong>
{
    private const string CheckpointStreamPrefix = "checkpoint";

    // Only the latest checkpoint is read; a few older ones are kept for diagnostics and the rest are scavenged.
    private const int MaxCheckpointsRetained = 5;

    private readonly KurrentDBClient _client;
    private readonly ConcurrentDictionary<string, bool> _configuredStreams = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBCheckpointStore" /> class.
    /// </summary>
    /// <param name="client">The KurrentDB client.</param>
    public KurrentDBCheckpointStore(KurrentDBClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<ulong> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default)
    {
        var streamName = ForCheckpoint(subscriptionName, partitionId);
        var result = _client.ReadStreamAsync(Direction.Backwards, streamName, StreamPosition.End, 1, cancellationToken: cancellationToken);
        if (await result.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
        {
            return default(ulong);
        }

        await foreach (var resolvedEvent in result.ConfigureAwait(false))
        {
            var checkpoint = JsonSerializer.Deserialize(resolvedEvent.Event.Data.Span, CheckpointJsonContext.Default.Checkpoint);
            return checkpoint?.Position ?? default(ulong);
        }

        return default(ulong);
    }

    /// <inheritdoc />
    public async Task StoreCheckpointAsync(string subscriptionName, string partitionId, ulong position, CancellationToken cancellationToken = default)
    {
        var streamName = ForCheckpoint(subscriptionName, partitionId);
        if (!_configuredStreams.ContainsKey(streamName))
        {
            await _client.SetStreamMetadataAsync(streamName, StreamState.Any, new StreamMetadata(maxCount: MaxCheckpointsRetained), cancellationToken: cancellationToken).ConfigureAwait(false);
            _configuredStreams.TryAdd(streamName, true);
        }
        Checkpoint checkpoint = new(streamName, position);
        EventData checkpointEventData = new(Uuid.NewUuid(), "$checkpoint", JsonSerializer.SerializeToUtf8Bytes(checkpoint, CheckpointJsonContext.Default.Checkpoint));
        await _client.AppendToStreamAsync(streamName, StreamState.Any, new List<EventData>() { checkpointEventData },
            null, null, null, cancellationToken).ConfigureAwait(false);
    }

    private static string ForCheckpoint(string subscriptionName, string partitionId)
        => $"{CheckpointStreamPrefix}-{subscriptionName}-{partitionId}";

    internal sealed record Checkpoint(string Id, ulong Position);
}

[JsonSerializable(typeof(KurrentDBCheckpointStore.Checkpoint))]
internal sealed partial class CheckpointJsonContext : JsonSerializerContext
{
}
