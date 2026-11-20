using Stateless;

namespace Continuum.Domain.Orleans.Tests;

public enum DoorStatus { Closed, Open }
public enum DoorCommand { Open, Close }

[GenerateSerializer]
public abstract class DoorEvent;
[GenerateSerializer]
public sealed class DoorOpened : DoorEvent;
[GenerateSerializer]
public sealed class DoorClosed : DoorEvent;

[GenerateSerializer]
public sealed class DoorState : EventDrivenStateMachineState<DoorState, DoorEvent, DoorStatus, DoorCommand>
{
    public DoorState() : base(DoorStatus.Closed) { }

    [Id(0)] public int OpenCount { get; set; }

    protected override void ConfigureEventHandlers()
    {
        On<DoorOpened>(_ =>
        {
            Status = DoorStatus.Open;
            OpenCount++;
        });
        On<DoorClosed>(_ => Status = DoorStatus.Closed);
    }

    protected override void ConfigureStateMachine()
    {
        StateMachine.Configure(DoorStatus.Closed).Permit(DoorCommand.Open, DoorStatus.Open);
        StateMachine.Configure(DoorStatus.Open).Permit(DoorCommand.Close, DoorStatus.Closed);
    }
}

public class EventDrivenStateMachineSessionTests
{
    private static EventDrivenStateMachineSession<DoorState, DoorEvent, DoorStatus, DoorCommand> Create(DoorState? s = null) => new(TestServices.Copier<DoorState>(), s ?? new DoorState());

    [Fact]
    public void Apply_HandlerSetsStatus()
    {
        using var session = Create();
        session.Apply(new DoorOpened());
        Assert.Equal(DoorStatus.Open, session.State.Status);
        Assert.Equal(DoorStatus.Open, session.State.StateMachine.State);
        Assert.Equal(1, session.State.OpenCount);
        Assert.Single(session.UncommittedEvents);
    }

    [Fact]
    public void Evolve_ReturnsPreviousAndCurrentState()
    {
        using var session = Create();
        var (previous, current) = session.Evolve(new DoorOpened());
        Assert.Equal(DoorStatus.Closed, previous.Status);
        Assert.Equal(DoorStatus.Open, current.Status);
    }

    [Fact]
    public void Apply_DoesNotFireStateMachine()
    {
        using var session = Create();
        Assert.False(session.State.StateMachine.CanFire(DoorCommand.Close));

        session.Apply(new DoorClosed());

        Assert.Equal(DoorStatus.Closed, session.State.Status);
        Assert.Single(session.UncommittedEvents);
    }

    [Fact]
    public void StateMachine_GuardsCommandsForHandlerSetStatus()
    {
        var state = new DoorState();
        Assert.True(state.StateMachine.CanFire(DoorCommand.Open));
        Assert.False(state.StateMachine.CanFire(DoorCommand.Close));

        state.When(new DoorOpened());

        Assert.False(state.StateMachine.CanFire(DoorCommand.Open));
        Assert.True(state.StateMachine.CanFire(DoorCommand.Close));
    }

    [Fact]
    public async Task DeepCopy_PreservesStatusAndTransitions()
    {
        var source = new DoorState();
        source.When(new DoorOpened());
        var copy = TestServices.Copier<DoorState>().Copy(source)!;
        Assert.Equal(DoorStatus.Open, copy.Status);
        Assert.Equal([DoorCommand.Close], await copy.StateMachine.GetPermittedTriggersAsync());
        Assert.Equal(1, copy.OpenCount);
    }

    [Fact]
    public void Session_CopiedStateMachineFollowsSessionStatus()
    {
        var source = new DoorState();
        source.When(new DoorOpened());
        using var session = Create(source);
        session.Apply(new DoorClosed());
        Assert.Equal(DoorStatus.Closed, session.State.Status);
        Assert.True(session.State.StateMachine.CanFire(DoorCommand.Open));
        Assert.Equal(DoorStatus.Open, source.Status);
    }
}
