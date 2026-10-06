namespace Continuum.Streaming.Orleans;

public interface IProjectionState
{
    IDictionary<string, IDictionary<string, StreamSequence>> LastSequenceByTopicThenPartition { get; }
    ValueTask<bool> WhenAsync(List<IStreamedEvent<object>> streamedEvents);
}

[GenerateSerializer]
public abstract class ProjectionState<TState> : IProjectionState
    where TState : ProjectionState<TState>
{
    [Id(0)] public IDictionary<string, IDictionary<string, StreamSequence>> LastSequenceByTopicThenPartition { get; }
        = new Dictionary<string, IDictionary<string, StreamSequence>>();

    public virtual async ValueTask<bool> WhenAsync(List<IStreamedEvent<object>> streamedEvents)
    {
        bool hasChanges = false;

        // ProjectionGrain only sends events for a single topic at a time and at least one event is guaranteed to be present.
        string topic = streamedEvents[0].Topic;
        if (!LastSequenceByTopicThenPartition.TryGetValue(topic, out IDictionary<string, StreamSequence>? lastSequenceByPartition))
        {
            LastSequenceByTopicThenPartition.Add(topic, lastSequenceByPartition = new Dictionary<string, StreamSequence>());
        }

        foreach (var streamedEvent in streamedEvents)
        {
            var currentSequence = new StreamSequence(streamedEvent.SequenceNumber, streamedEvent.SubSequenceNumber);
            var eventHasNotBeenHandled = lastSequenceByPartition.TryGetValue(streamedEvent.PartitionId, out StreamSequence lastSequence)
                ? currentSequence > lastSequence
                : true;
            if (eventHasNotBeenHandled)
            {
                hasChanges |= await Handle(streamedEvent);
                lastSequenceByPartition[streamedEvent.PartitionId] = currentSequence;
            }
        }

        return hasChanges;
    }

    /// <summary>
    /// Handles the given event by updating the state.
    /// </summary>
    /// <param name="streamedEvent">The event to handle.</param>
    /// <returns>True if the event was applied and false if the event was ignored.</returns>
    protected abstract ValueTask<bool> Handle(IStreamedEvent<object> streamedEvent);
}


public interface IProjectionState<TEventArgs>
{
    IDictionary<string, IDictionary<string, StreamSequence>> LastSequenceByTopicThenPartition { get; }
    ValueTask<bool> WhenAsync(List<IStreamedEvent<object>> streamedEvents, TEventArgs eventArgs);
}

[GenerateSerializer]
public abstract class ProjectionState<TState, TEventArgs> : IProjectionState<TEventArgs>
    where TState : ProjectionState<TState, TEventArgs>
{
    [Id(0)]
    public IDictionary<string, IDictionary<string, StreamSequence>> LastSequenceByTopicThenPartition { get; }
        = new Dictionary<string, IDictionary<string, StreamSequence>>();

    public virtual async ValueTask<bool> WhenAsync(List<IStreamedEvent<object>> streamedEvents, TEventArgs eventArgs)
    {
        bool hasChanges = false;

        // ProjectionGrain only sends events for a single topic at a time and at least one event is guaranteed to be present.
        string topic = streamedEvents[0].Topic;
        if (!LastSequenceByTopicThenPartition.TryGetValue(topic, out IDictionary<string, StreamSequence>? lastSequenceByPartition))
        {
            LastSequenceByTopicThenPartition.Add(topic, lastSequenceByPartition = new Dictionary<string, StreamSequence>());
        }

        foreach (var streamedEvent in streamedEvents)
        {
            var currentSequence = new StreamSequence(streamedEvent.SequenceNumber, streamedEvent.SubSequenceNumber);
            var eventHasNotBeenHandled = lastSequenceByPartition.TryGetValue(streamedEvent.PartitionId, out StreamSequence lastSequence)
                ? currentSequence > lastSequence
                : true;
            if (eventHasNotBeenHandled)
            {
                hasChanges |= await Handle(streamedEvent, eventArgs);
                lastSequenceByPartition[streamedEvent.PartitionId] = currentSequence;
            }
        }

        return hasChanges;
    }

    /// <summary>
    /// Handles the given event by updating the state.
    /// </summary>
    /// <param name="streamedEvent">The event to handle.</param>
    /// <param name="eventArgs">Additional arguments for handling the event.</param>
    /// <returns>True if the event was applied and false if the event was ignored.</returns>
    protected abstract ValueTask<bool> Handle(IStreamedEvent<object> streamedEvent, TEventArgs eventArgs);
}
