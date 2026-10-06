using System.Text.Json;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB;

public sealed class KurrentDBCheckpointStore : ICheckpointStore<ulong>
{
    private const string CheckpointStreamPrefix = "checkpoint";

    private readonly KurrentDBClient _client;

    public KurrentDBCheckpointStore(KurrentDBClient client)
    {
        _client = client;
    }

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

        var checkpoint = JsonSerializer.Deserialize<Checkpoint>(resolvedEvent.Event.Data.Span);
        if (checkpoint is null)
        {
            return default(ulong);
        }

        return checkpoint.Position;
    }

    public Task StoreCheckpointAsync(string subscriptionName, ulong position, CancellationToken cancellationToken = default)
    {
        var streamName = StreamName.ForCheckpoint(subscriptionName);
        Checkpoint checkpoint = new(streamName, position);
        EventData checkpointEventData = new(Uuid.NewUuid(), "$checkpoint", JsonSerializer.SerializeToUtf8Bytes(checkpoint));
        return _client.AppendToStreamAsync(streamName, StreamState.Any, new List<EventData>() { checkpointEventData },
            null, null, null, cancellationToken);
    }

    private sealed record Checkpoint(string Id, ulong Position);
}
