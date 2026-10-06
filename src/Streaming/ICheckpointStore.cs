namespace Continuum.Streaming;

public interface ICheckpointStore<TStreamPosition>
{
    Task<TStreamPosition> GetLastCheckpointAsync(string subscriptionName, CancellationToken cancellationToken = default);
    Task StoreCheckpointAsync(string subscriptionName, TStreamPosition streamPosition, CancellationToken cancellationToken = default);
}
