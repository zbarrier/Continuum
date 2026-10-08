namespace Continuum.Streaming.Orleans;

/// <summary>
/// <see cref="ICheckpointStore{TStreamPosition}"/> backed by <see cref="SubscriptionCheckpointGrain{TPosition}"/>, so
/// checkpoints can be persisted with any Orleans grain storage provider.
/// </summary>
/// <remarks>
/// Each subscription partition is its own grain, keyed <c>{subscriptionName}/{partitionId}</c>.
/// </remarks>
/// <typeparam name="TPosition">The provider-specific stream position type.</typeparam>
/// <param name="grainFactory">The grain factory.</param>
public sealed class SubscriptionCheckpointStore<TPosition>(IGrainFactory grainFactory) : ICheckpointStore<TPosition>
    where TPosition : IComparable<TPosition>
{
    private readonly IGrainFactory _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));

    /// <inheritdoc/>
    public Task<TPosition> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default)
        => GetGrain(subscriptionName, partitionId).GetCheckpointAsync(cancellationToken);

    /// <inheritdoc/>
    public Task StoreCheckpointAsync(string subscriptionName, string partitionId, TPosition streamPosition, CancellationToken cancellationToken = default)
        => GetGrain(subscriptionName, partitionId).StoreCheckpointAsync(streamPosition, cancellationToken);

    private ISubscriptionCheckpointGrain<TPosition> GetGrain(string subscriptionName, string partitionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(subscriptionName);
        ArgumentException.ThrowIfNullOrEmpty(partitionId);
        return _grainFactory.GetGrain<ISubscriptionCheckpointGrain<TPosition>>($"{subscriptionName}/{partitionId}");
    }
}
