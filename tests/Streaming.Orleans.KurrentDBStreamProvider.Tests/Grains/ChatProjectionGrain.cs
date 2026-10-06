using Continuum.Streaming.Orleans;

using Microsoft.Extensions.Logging;

using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

/// <summary>
///     A projection built on <see cref="StreamProjectionGrain{TGrain, TState}" />, used to prove the base class
///     subscribes, applies events to its persistent state and writes them.
/// </summary>
/// <remarks>
///     The grain subscribes to the stream whose key matches its own primary key, which is the convention implicit
///     subscriptions use. It is activated explicitly by the test rather than by an implicit subscription attribute,
///     so that the assertions are about the base class rather than about Orleans' implicit activation.
/// </remarks>
public class ChatProjectionGrain : StreamProjectionGrain<ChatProjectionGrain, ChatProjectionState>, IChatProjectionGrain
{
    private readonly IPersistentState<ChatProjectionState> _state;

    public ChatProjectionGrain(
        [PersistentState("ChatProjection", "MemoryStorageProvider")] IPersistentState<ChatProjectionState> state,
        ILogger<ChatProjectionGrain> logger)
    {
        _state = state;
        Logger = logger;
    }

    protected override IPersistentState<ChatProjectionState> State => _state;

    protected override ILogger Logger { get; }

    protected override IEnumerable<StreamSubscriptionSource> Subscriptions =>
    [
        new(Constants.AllStreamProviderName, StreamIdFromKey(Constants.EventSourcedStreamPrefix))
    ];

    public Task<string[]> GetAuthors() => Task.FromResult(_state.State.Authors.ToArray());

    public Task<int> GetAppliedCount() => Task.FromResult(_state.State.AppliedCount);
}

[GenerateSerializer]
public class ChatProjectionState : StreamProjectionState<ChatProjectionState>
{
    [Id(0)] public List<string> Authors { get; init; } = new();

    [Id(1)] public int AppliedCount { get; set; }

    protected override ValueTask<bool> Handle(IStreamedEvent<object> streamedEvent)
    {
        if (streamedEvent.Event is not ChatMessage message)
        {
            return ValueTask.FromResult(false);
        }

        Authors.Add(message.Author);
        AppliedCount++;
        return ValueTask.FromResult(true);
    }
}
