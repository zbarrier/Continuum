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

    private readonly KurrentDBClient _client;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBCheckpointStore" /> class.
    /// </summary>
    /// <param name="client">The KurrentDB client.</param>
    public KurrentDBCheckpointStore(KurrentDBClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<ulong> GetLastCheckpointAsync(string subscriptionName, CancellationToken cancellationToken = default)
    {
        var streamName = StreamName.ForCheckpoint(subscriptionName);
        var result = _client.ReadStreamAsync(Direction.Backwards, streamName, StreamPosition.End, 1);
        if (await result.ReadState == ReadState.StreamNotFound)
        {
            return default(ulong);
        }

        var resolvedEvent = await result.FirstAsync();
        if (resolvedEvent.Equals(default(ResolvedEvent)))
        {
            // StoreCheckpointAsync derives the stream name itself, so pass the subscription name and not the
            // already prefixed stream name, otherwise the checkpoint lands in 'checkpoint-checkpoint-{name}'.
            await StoreCheckpointAsync(subscriptionName, Position.Start.CommitPosition, cancellationToken);
            return default(ulong);
        }

        var checkpoint = JsonSerializer.Deserialize(resolvedEvent.Event.Data.Span, CheckpointJsonContext.Default.Checkpoint);
        if (checkpoint is null)
        {
            return default(ulong);
        }

        return checkpoint.Position;
    }

    /// <inheritdoc />
    public Task StoreCheckpointAsync(string subscriptionName, ulong position, CancellationToken cancellationToken = default)
    {
        var streamName = StreamName.ForCheckpoint(subscriptionName);
        Checkpoint checkpoint = new(streamName, position);
        EventData checkpointEventData = new(Uuid.NewUuid(), "$checkpoint", JsonSerializer.SerializeToUtf8Bytes(checkpoint, CheckpointJsonContext.Default.Checkpoint));
        return _client.AppendToStreamAsync(streamName, StreamState.Any, new List<EventData>() { checkpointEventData },
            null, null, null, cancellationToken);
    }

    internal sealed record Checkpoint(string Id, ulong Position);
}

[JsonSerializable(typeof(KurrentDBCheckpointStore.Checkpoint))]
internal sealed partial class CheckpointJsonContext : JsonSerializerContext
{
}
