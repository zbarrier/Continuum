using Stateless;

namespace Continuum.Domain.Orleans.Tests;

public enum DoorStatus { Closed, Open }
public enum DoorCommand { Open, Close }

[GenerateSerializer]
public abstract class DoorEvent(DoorCommand command) : StateMachineEvent<DoorCommand>(command);
[GenerateSerializer]
public sealed class DoorOpened() : DoorEvent(DoorCommand.Open);
[GenerateSerializer]
public sealed class DoorClosed() : DoorEvent(DoorCommand.Close);

[GenerateSerializer]
public sealed class DoorState : EventDrivenStateMachineState<DoorState, DoorEvent, DoorStatus, DoorCommand>
{
    public DoorState() : base(() => new StateMachine<DoorStatus, DoorCommand>(DoorStatus.Closed)) { }

    [Id(0)] public int OpenCount { get; set; }

    protected override void ConfigureEventHandlers() => On<DoorOpened>(_ => OpenCount++);

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
    public void Apply_FiresCommandAndInvokesHandler()
    {
        using var session = Create();
        session.Apply(new DoorOpened());
        Assert.Equal(DoorStatus.Open, session.State.StateMachine.State);
        Assert.Equal(1, session.State.OpenCount);
        Assert.Single(session.UncommittedEvents);
    }

    [Fact]
    public void Evolve_ReturnsPreviousAndCurrentState()
    {
        using var session = Create();
        var (previous, current) = session.Evolve(new DoorOpened());
        Assert.Equal(DoorStatus.Closed, previous.StateMachine.State);
        Assert.Equal(DoorStatus.Open, current.StateMachine.State);
    }

    [Fact]
    public void Apply_InvalidTransition_Throws()
    {
        using var session = Create();
        Assert.Throws<InvalidOperationException>(() => session.Apply(new DoorClosed()));
    }

    [Fact]
    public async Task DeepCopy_DoesNotPreserveStateMachineConfiguration()
    {
        var source = new DoorState();
        source.When(new DoorOpened());
        var copy = TestServices.Copier<DoorState>().Copy(source)!;
        Assert.Equal(DoorStatus.Open, copy.StateMachine.State);
        Assert.Empty(await copy.StateMachine.GetPermittedTriggersAsync());
    }

    [Fact]
    public void Session_ReconfiguresCopiedStateMachine()
    {
        var source = new DoorState();
        source.When(new DoorOpened());
        using var session = Create(source);
        session.Apply(new DoorClosed());
        Assert.Equal(DoorStatus.Closed, session.State.StateMachine.State);
    }
}
