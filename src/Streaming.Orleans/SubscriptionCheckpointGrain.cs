using Microsoft.Extensions.Logging;

using Orleans.Concurrency;
using Orleans.Runtime;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// Constants shared by every <see cref="SubscriptionCheckpointGrain{TPosition}"/>.
/// </summary>
public static class SubscriptionCheckpointGrain
{
    /// <summary>
    /// The name of the grain storage provider the checkpoint is persisted with.
    /// </summary>
    public const string StorageName = "SubscriptionCheckpointStore";

    /// <summary>
    /// How often a changed checkpoint is written to grain storage.
    /// </summary>
    public static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(20);
}

/// <summary>
/// Persists a subscription checkpoint through Orleans grain storage, writing behind the caller.
/// </summary>
/// <typeparam name="TPosition">The provider-specific stream position type.</typeparam>
/// <remarks>
/// <para>
///     <see cref="StoreCheckpointAsync"/> only updates the checkpoint in memory, so callers never wait for storage. A
///     timer writes the checkpoint every <see cref="SubscriptionCheckpointGrain.FlushInterval"/> when it has changed,
///     and deactivation writes any change still pending.
/// </para>
/// <para>
///     The grain is reentrant so a subscription storing a checkpoint is never held up by an in-progress write. Writes
///     are serialized so the timer and deactivation never write concurrently.
/// </para>
/// <para>
///     A silo crash can lose up to one interval of progress. The subscription then resumes from the last written
///     checkpoint and redelivers those events, which projections already ignore through their per-stream sequence
///     watermark.
/// </para>
/// <para>
///     The grain storage provider must be registered under <see cref="SubscriptionCheckpointGrain.StorageName"/>.
/// </para>
/// </remarks>
[Reentrant]
public sealed class SubscriptionCheckpointGrain<TPosition> : Grain, ISubscriptionCheckpointGrain<TPosition>
    where TPosition : IComparable<TPosition>
{
    private readonly IPersistentState<SubscriptionCheckpointState<TPosition>> _storage;
    private readonly ILogger<SubscriptionCheckpointGrain<TPosition>> _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private bool _isDirty;

    /// <param name="storage">The persisted checkpoint.</param>
    /// <param name="logger">The logger.</param>
    public SubscriptionCheckpointGrain(
        [PersistentState("checkpoint", SubscriptionCheckpointGrain.StorageName)] IPersistentState<SubscriptionCheckpointState<TPosition>> storage,
        ILogger<SubscriptionCheckpointGrain<TPosition>> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    private SubscriptionCheckpointState<TPosition> State => _storage.State;

    /// <inheritdoc/>
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.RegisterGrainTimer(FlushAsync, new GrainTimerCreationOptions(SubscriptionCheckpointGrain.FlushInterval, SubscriptionCheckpointGrain.FlushInterval)
        {
            Interleave = true,
        });
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        try
        {
            await FlushAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write checkpoint {Position} for subscription {Subscription} on deactivation.",
                State.Position, this.GetPrimaryKeyString());
        }
    }

    /// <inheritdoc/>
    public Task<TPosition> GetCheckpointAsync(CancellationToken cancellationToken = default) => Task.FromResult(State.Position);

    /// <inheritdoc/>
    public Task StoreCheckpointAsync(TPosition position, CancellationToken cancellationToken = default)
    {
        if (State.Position is null || position.CompareTo(State.Position) > 0)
        {
            State.Position = position;
            _isDirty = true;
        }
        return Task.CompletedTask;
    }

    private async Task FlushAsync(CancellationToken cancellationToken)
    {
        if (!_isDirty)
        {
            return;
        }

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            // Another flush may have written the checkpoint while this one waited.
            if (!_isDirty)
            {
                return;
            }

            var written = State.Position;
            await _storage.WriteStateAsync(cancellationToken);

            // A checkpoint stored during the write stays dirty for the next flush.
            _isDirty = State.Position.CompareTo(written) != 0;
        }
        finally
        {
            _writeLock.Release();
        }
    }
}

/// <summary>
/// The persisted checkpoint of a <see cref="SubscriptionCheckpointGrain{TPosition}"/>.
/// </summary>
/// <typeparam name="TPosition">The provider-specific stream position type.</typeparam>
[Alias("Continuum.Streaming.SubscriptionCheckpointState.V1`1"), GenerateSerializer]
public sealed class SubscriptionCheckpointState<TPosition>
{
    /// <summary>
    /// The last processed position.
    /// </summary>
    [Id(0)]
    public TPosition Position { get; set; } = default!;
}
