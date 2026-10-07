using Continuum.Streaming.Orleans;

using Microsoft.Extensions.Logging;

using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

/// <summary>
///     A projection that explicitly subscribes to more than one stream, proving a single grain can consume several.
/// </summary>
/// <remarks>
///     The two streams share a namespace and differ by key, because the <c>$all</c> provider is filtered to a single
///     stream name prefix and that prefix is the namespace. That is incidental to what is under test: the base class
///     holds one handle per subscription regardless of whether the streams differ by namespace or by key, and a
///     single shared handle would show only one of the two streams here.
/// </remarks>
public class MultiStreamProjectionGrain
    : ProjectionGrain<MultiStreamProjectionGrain, MultiStreamProjectionState>, IMultiStreamProjectionGrain
{
    public MultiStreamProjectionGrain(
        [PersistentState("MultiStreamProjection", "MemoryStorageProvider")] IPersistentState<MultiStreamProjectionState> storage,
        ILogger<MultiStreamProjectionGrain> logger)
        : base(storage, logger)
    {
    }

    protected override IEnumerable<StreamSubscriptionSource> Subscriptions =>
    [
        new(Constants.AllStreamProviderName, StreamId.Create(Constants.EventSourcedStreamPrefix, FirstStreamKey(this.GetPrimaryKeyString()))),
        new(Constants.AllStreamProviderName, StreamId.Create(Constants.EventSourcedStreamPrefix, SecondStreamKey(this.GetPrimaryKeyString())))
    ];

    public static string FirstStreamKey(string key) => $"{key}.first";

    public static string SecondStreamKey(string key) => $"{key}.second";

    public Task<string[]> GetAuthors() => Task.FromResult(State.Authors.ToArray());
}

[GenerateSerializer]
public class MultiStreamProjectionState : ProjectionState<MultiStreamProjectionState>
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
