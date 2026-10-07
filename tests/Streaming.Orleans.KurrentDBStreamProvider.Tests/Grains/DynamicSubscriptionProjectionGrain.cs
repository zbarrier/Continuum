using Continuum.Streaming.Orleans;

using Microsoft.Extensions.Logging;

using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

public interface IDynamicSubscriptionProjectionGrain : IGrainWithStringKey
{
    Task Follow(string streamKey);

    Task Unfollow(string streamKey);

    Task<string[]> GetAuthors();
}

/// <summary>
///     A projection that starts with no subscriptions and follows streams chosen at runtime, used to prove the
///     per-stream subscribe and unsubscribe methods on <see cref="ProjectionGrain{TGrain, TState}" />.
/// </summary>
/// <remarks>
///     The base class records each followed stream in the projection state, which is what restores it when the
///     grain reactivates.
/// </remarks>
public class DynamicSubscriptionProjectionGrain
    : ProjectionGrain<DynamicSubscriptionProjectionGrain, DynamicSubscriptionProjectionState>, IDynamicSubscriptionProjectionGrain
{
    public DynamicSubscriptionProjectionGrain(
        [PersistentState("DynamicSubscriptionProjection", "MemoryStorageProvider")] IPersistentState<DynamicSubscriptionProjectionState> storage,
        ILogger<DynamicSubscriptionProjectionGrain> logger)
        : base(storage, logger)
    {
    }

    public Task Follow(string streamKey) => SubscribeAsync(SourceFor(streamKey));

    public Task Unfollow(string streamKey) => UnsubscribeAsync(SourceFor(streamKey));

    public Task<string[]> GetAuthors() => Task.FromResult(State.Authors.ToArray());

    private static StreamSubscriptionSource SourceFor(string streamKey)
        => new(Constants.AllStreamProviderName, StreamId.Create(Constants.EventSourcedStreamPrefix, streamKey));
}

[GenerateSerializer]
public class DynamicSubscriptionProjectionState : ProjectionState<DynamicSubscriptionProjectionState>
{
    [Id(0)] public List<string> Authors { get; init; } = new();

    protected override ValueTask<bool> Handle(IStreamedEvent<object> streamedEvent)
    {
        if (streamedEvent.Event is not ChatMessage message)
        {
            return ValueTask.FromResult(false);
        }

        Authors.Add(message.Author);
        return ValueTask.FromResult(true);
    }
}
