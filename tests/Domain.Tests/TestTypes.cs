using Continuum.CSharpFunctionalExtensions;

using Microsoft.Extensions.Logging;

namespace Continuum.Domain.Tests;

public abstract record CounterEvent;
public sealed record CounterIncremented(int By) : CounterEvent;

public sealed class CounterState : IEventDrivenState<CounterState, CounterEvent>
{
    public int Count { get; private set; }
    public void When(CounterEvent @event) { if (@event is CounterIncremented e) Count += e.By; }
}

public sealed class FakeSession : IEventDrivenSession<CounterState, CounterEvent>
{
    private readonly List<CounterEvent> _events = [];
    public CounterState State { get; } = new();
    public IReadOnlyList<CounterEvent> UncommittedEvents => _events;
    public bool Disposed { get; private set; }
    public void Apply<TEvent>(TEvent evt) where TEvent : CounterEvent { State.When(evt); _events.Add(evt); }
    public (CounterState PreviousState, CounterState CurrentState) Evolve<TEvent>(TEvent evt) where TEvent : CounterEvent { Apply(evt); return (State, State); }
    public void Dispose() => Disposed = true;
}

public sealed class FakeChange(object evt) : IChange { public string EventType => Event.GetType().Name; public object Event { get; } = evt; }
public sealed class FakeSaveChangesResponse(IEnumerable<IChange> changes, int version) : ISaveChangesResponse { public IEnumerable<IChange> Changes { get; } = changes; public int Version { get; } = version; }

public sealed class FakeScope : IEventDrivenSessionScope<CounterState, CounterEvent>
{
    public RecordingLogger Logger { get; } = new();
    ILogger IEventDrivenSessionScope<CounterState, CounterEvent>.Logger => Logger;
    public int SessionsCreated { get; private set; }
    public int SaveCount { get; private set; }
    public FakeSession? LastSession { get; private set; }
    public IEventDrivenSession<CounterState, CounterEvent> CreateSession() { SessionsCreated++; return LastSession = new FakeSession(); }
    public Task<Result<ISaveChangesResponse>> SaveChanges(IEventDrivenSession<CounterState, CounterEvent> session)
    {
        SaveCount++;
        ISaveChangesResponse response = new FakeSaveChangesResponse(session.UncommittedEvents.Select(e => (IChange)new FakeChange(e)).ToList(), session.UncommittedEvents.Count);
        return Task.FromResult(Result.Success(response));
    }
}

public sealed class RecordingLogger : ILogger
{
    public List<Exception> Exceptions { get; } = [];
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { if (exception is not null) Exceptions.Add(exception); }
}
