using Orleans.Runtime;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// A <see cref="StreamSubscriberGrain{TGrain}"/> that maintains a single persistent projection state.
/// </summary>
/// <remarks>
/// Unlike <see cref="ProjectionGrain{TGrain}"/>, this base holds exactly one persistent state. Stream
/// delivery is single-event, so fanning out across several states would cost one write per state per
/// event with no amortization; prefer a separate grain per read model instead.
/// </remarks>
public abstract class StreamProjectionGrain<TGrain, TState> : StreamSubscriberGrain<TGrain>
    where TGrain : StreamProjectionGrain<TGrain, TState>
    where TState : class, IStreamProjectionState, new()
{
    /// <summary>
    /// The persistent state this projection maintains.
    /// </summary>
    protected abstract IPersistentState<TState> State { get; }

    /// <summary>
    /// Applies the event to <see cref="State"/> and writes it when the state changed.
    /// </summary>
    /// <remarks>
    /// The state is written before delivery is acknowledged, so a failure here propagates and the event
    /// is not treated as consumed. The sequence watermark is only durable once the write succeeds, which
    /// is what makes redelivery safe.
    /// </remarks>
    protected override async Task OnNextAsync(IStreamedEvent<object> streamedEvent, StreamSequenceToken? token)
    {
        var hasChanges = await State.State.WhenAsync(streamedEvent);
        if (hasChanges)
        {
            await State.WriteStateAsync();
        }
    }
}
