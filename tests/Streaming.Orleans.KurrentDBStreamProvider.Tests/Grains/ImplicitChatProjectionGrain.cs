using Continuum.Streaming.Orleans;

using Microsoft.Extensions.Logging;

using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

/// <summary>
///     A projection that is activated by the runtime when an event arrives on its stream, rather than by a caller.
/// </summary>
/// <remarks>
///     The attribute names the stream namespace, and the runtime activates the grain whose key equals the stream key
///     and then supplies the subscription through <c>OnSubscribed</c>. The grain therefore declares no explicit
///     subscription: the provider and stream are both chosen by the runtime, so overriding
///     <see cref="StreamSubscriberGrain{TGrain}.Subscriptions" /> would create a second, duplicate subscription.
/// </remarks>
[ImplicitStreamSubscription(Constants.ImplicitStreamPrefix)]
public class ImplicitChatProjectionGrain
    : StreamProjectionGrain<ImplicitChatProjectionGrain, ImplicitChatProjectionState>, IImplicitChatProjectionGrain
{
    private readonly IPersistentState<ImplicitChatProjectionState> _state;

    public ImplicitChatProjectionGrain(
        [PersistentState("ImplicitChatProjection", "MemoryStorageProvider")] IPersistentState<ImplicitChatProjectionState> state,
        ILogger<ImplicitChatProjectionGrain> logger)
    {
        _state = state;
        Logger = logger;
    }

    protected override IPersistentState<ImplicitChatProjectionState> State => _state;

    protected override ILogger Logger { get; }

    public Task<string[]> GetAuthors() => Task.FromResult(_state.State.Authors.ToArray());
}

/// <summary>
///     Projection state for <see cref="ImplicitChatProjectionGrain" />.
/// </summary>
/// <remarks>
///     Deliberately its own type rather than a reuse of the explicitly subscribed grain's state. The two grains are
///     exercised by unrelated tests, so sharing a state type would let a change made for one silently alter the other,
///     and it would give both grains the same Orleans serializer identity for what are logically separate read models.
/// </remarks>
[GenerateSerializer]
public class ImplicitChatProjectionState : StreamProjectionState<ImplicitChatProjectionState>
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
