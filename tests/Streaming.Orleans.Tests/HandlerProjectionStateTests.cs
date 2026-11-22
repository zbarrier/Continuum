using System.Numerics;

using Continuum.Orleans.Streams;

namespace Continuum.Streaming.Orleans.Tests;

/// <summary>
///     Verifies how <see cref="HandlerProjectionState{TState}" /> dispatches events to registered handlers.
/// </summary>
public class HandlerProjectionStateTests
{
    private long _sequenceNumber;

    [Fact]
    public async Task Invokes_The_Handler_Registered_For_The_Event_Type()
    {
        var state = new RecordingState();

        var hasChanges = await state.WhenAsync(Event(new Started("a")));

        Assert.True(hasChanges);
        Assert.Equal(new[] { "started a" }, state.Log);
    }

    [Fact]
    public async Task Returns_The_Handlers_Result_So_An_Unchanged_State_Is_Not_Written()
    {
        var state = new RecordingState();

        var hasChanges = await state.WhenAsync(Event(new Removed("missing")));

        Assert.False(hasChanges);
        Assert.Equal(new[] { "not removed missing" }, state.Log);
    }

    [Fact]
    public async Task Awaits_An_Asynchronous_Handler()
    {
        var state = new RecordingState();

        var hasChanges = await state.WhenAsync(Event(new Looked("a")));

        Assert.True(hasChanges);
        Assert.Equal(new[] { "looked a" }, state.Log);
    }

    [Fact]
    public async Task Passes_The_Envelope_To_The_Handler()
    {
        var state = new RecordingState();
        var streamedEvent = Event(new Started("a"));

        await state.WhenAsync(streamedEvent);

        Assert.Same(streamedEvent, state.LastEnvelope);
    }

    [Fact]
    public async Task Ignores_An_Event_Without_A_Handler_For_Its_Exact_Type()
    {
        var state = new RecordingState();

        // DerivedStarted is a Started, but only the exact runtime type is matched.
        var hasChanges = await state.WhenAsync([Event(new Unhandled()), Event(new DerivedStarted("a"))]);

        Assert.False(hasChanges);
        Assert.Empty(state.Log);
    }

    [Fact]
    public void Rejects_A_Second_Handler_For_The_Same_Event_Type()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => new DuplicateHandlerState());

        Assert.Contains(nameof(Started), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Passes_The_Context_To_Synchronous_And_Asynchronous_Handlers()
    {
        var state = new ContextState();
        var context = new LookupContext(new Dictionary<string, string> { ["a"] = "Alpha", ["b"] = "Beta" });

        var hasChanges = await state.WhenAsync([Event(new Started("a")), Event(new Looked("b"))], context);

        Assert.True(hasChanges);
        Assert.Equal(new[] { "started Alpha", "looked Beta" }, state.Log);
    }

    [Fact]
    public async Task Rejects_A_Missing_Context()
    {
        var state = new ContextState();

        await Assert.ThrowsAsync<ArgumentNullException>(() => state.WhenAsync(Event(new Started("a")), null!));
    }

    private StreamedEvent<object> Event(object payload)
    {
        _sequenceNumber++;
        return new StreamedEvent<object>(
            Guid.CreateVersion7(),
            payload.GetType().Name,
            streamName: new StreamName("recorded-1", "recorded", null, "1"),
            streamVersion: 0,
            cursor: new StreamedEventCursor("store", "recorded", "0", new BigInteger(_sequenceNumber), 0),
            timestamp: DateTime.UtcNow,
            @event: payload);
    }

    private record Started(string Id);

    private sealed record DerivedStarted(string Id) : Started(Id);

    private sealed record Removed(string Id);

    private sealed record Looked(string Id);

    private sealed record Unhandled;

    private sealed class RecordingState : HandlerProjectionState<RecordingState>
    {
        public RecordingState()
        {
            On<Started>((evt, envelope) =>
            {
                Log.Add($"started {evt.Id}");
                LastEnvelope = envelope;
                return true;
            });
            On<Removed>((evt, _) =>
            {
                Log.Add($"not removed {evt.Id}");
                return false;
            });
            On<Looked>(async (evt, _) =>
            {
                await Task.Yield();
                Log.Add($"looked {evt.Id}");
                return true;
            });
        }

        public List<string> Log { get; } = [];

        public IStreamedEvent<object>? LastEnvelope { get; private set; }
    }

    private sealed class DuplicateHandlerState : HandlerProjectionState<DuplicateHandlerState>
    {
        public DuplicateHandlerState()
        {
            On<Started>((_, _) => true);
            On<Started>((_, _) => false);
        }
    }

    private sealed record LookupContext(IReadOnlyDictionary<string, string> Names);

    private sealed class ContextState : HandlerProjectionState<ContextState, LookupContext>
    {
        public ContextState()
        {
            On<Started>((evt, _, context) =>
            {
                Log.Add($"started {context.Names[evt.Id]}");
                return true;
            });
            On<Looked>(async (evt, _, context) =>
            {
                await Task.Yield();
                Log.Add($"looked {context.Names[evt.Id]}");
                return true;
            });
        }

        public List<string> Log { get; } = [];
    }
}
