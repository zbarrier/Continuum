using Orleans.Core;

namespace Continuum.Streaming.Orleans;

public interface IProjectionGrain : IGrainWithStringKey
{
    Task OnNextBatchAsync([Immutable] List<IStreamedEvent<object>> streamingEvents);
}

public abstract class ProjectionGrain<TGrain> : Grain
    where TGrain : ProjectionGrain<TGrain>
{
    protected abstract int NumberOfPersistentStates { get; }
    protected abstract ValueTask<bool> OnWhenAsync(int index, List<IStreamedEvent<object>> streamEvents);
    protected abstract Task OnWriteStateAsync(int index);

    public virtual async Task OnNextBatchAsync([Immutable] List<IStreamedEvent<object>> streamEvents)
    {
        if (streamEvents is null || streamEvents.Count == 0)
        {
            return;
        }

        // WARNING: A ValueTask can only be awaited once. If you need to await the same ValueTask
        // multiple times, you should call AsTask to obtain a new Task object.
        var valueTasks = new List<ValueTask<bool>>(NumberOfPersistentStates);
        for (int i = 0; i < NumberOfPersistentStates; i++)
        {
            valueTasks.Add(OnWhenAsync(i, streamEvents));
        }

        var tasks = new List<Task>(NumberOfPersistentStates);
        for (int i = 0; i < NumberOfPersistentStates; i++)
        {
            var hasChanges = await valueTasks[i];
            if (hasChanges)
            {
                tasks.Add(OnWriteStateAsync(i));
            }
        }

        await Task.WhenAll(tasks);
    }
}
