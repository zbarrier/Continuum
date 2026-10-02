namespace Continuum.Domain.Orleans.Tests;

public class EventDrivenSessionTests
{
    private static EventDrivenSession<CounterState, CounterEvent> Create(CounterState? s = null) => new("counter-1", TestServices.Copier<CounterState>(), s ?? new CounterState());

    [Fact]
    public void Apply_UpdatesStateAndTracksEvent()
    {
        using var session = Create();
        session.Apply(new CounterIncremented(2));
        session.Apply(new CounterIncremented(3));
        Assert.Equal(5, session.State.Count);
        Assert.Equal(2, session.UncommittedEvents.Count);
    }

    [Fact]
    public void Constructor_CopiesSourceState()
    {
        var source = new CounterState { Count = 1 };
        using var session = Create(source);
        session.Apply(new CounterIncremented(4));
        Assert.Equal(1, source.Count);
        Assert.Equal(5, session.State.Count);
    }

    [Fact]
    public void Evolve_ReturnsPreviousAndCurrentState()
    {
        using var session = Create();
        session.Apply(new CounterIncremented(2));
        var (previous, current) = session.Evolve(new CounterReset());
        Assert.Equal(2, previous.Count);
        Assert.Equal(0, current.Count);
        Assert.Equal(2, session.UncommittedEvents.Count);
    }
}
