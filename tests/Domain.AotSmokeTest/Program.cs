using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Stateless;

// Exercises Continuum.Domain and Continuum.Domain.Orleans under Native AOT. Publish with:
//   dotnet publish tests/Domain.AotSmokeTest -c Release
// then run the produced executable. A non-zero exit code means a check failed.

var failures = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (!condition) failures++;
}

// Orleans DeepCopier (AddSerializer) is not Native AOT compatible upstream, so sessions are not exercised here;
// state handler dispatch and the Stateless state machine are exercised directly.
var counter = new CounterState();
counter.When(new CounterIncremented(2));
counter.When(new CounterIncremented(3));
Check("State dispatches events", counter.Count == 5);

var door = new DoorState();
door.When(new DoorOpened());
door.When(new DoorClosed());
Check("State machine transitions", door.StateMachine.State == DoorStatus.Closed && door.OpenCount == 1);

// SaveChangesResponseConverter with source-generated JSON metadata
var mapper = new SmokeTypeMapper();
var options = new JsonSerializerOptions { TypeInfoResolver = SmokeJsonContext.Default };
options.Converters.Add(new SaveChangesResponseConverter(mapper));
var responseInfo = (JsonTypeInfo<SaveChangesResponse>)options.GetTypeInfo(typeof(SaveChangesResponse));
var json = JsonSerializer.Serialize(new SaveChangesResponse([new Change("smoke.counter-incremented", new CounterIncremented(7))], 4), responseInfo);
var back = JsonSerializer.Deserialize(json, responseInfo);
Check("SaveChangesResponse round-trip", back is { Version: 4 } && back.Changes.Single().Event is CounterIncremented { By: 7 });

// Result pipeline extensions
var scope = new SmokeScope();
var saved = await Result.Success().BindWithSessionScope(scope, s => s.Apply(new CounterIncremented(1)));
Check("BindWithSessionScope saves", saved.IsSuccess && saved.Value.Version == 1);
var failed = await Result.Success().BindWithSessionScope(scope, (Action<IEventDrivenSession<CounterState, CounterEvent>>)(_ => throw new InvalidOperationException("boom")));
Check("BindWithSessionScope converts exceptions", failed.IsFailure);

Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) failed.");
return failures == 0 ? 0 : 1;

[GenerateSerializer]
internal abstract class CounterEvent;

[GenerateSerializer]
internal sealed class CounterIncremented(int by) : CounterEvent
{
    [Id(0)] public int By { get; set; } = by;
}

[GenerateSerializer]
internal sealed class CounterState : EventDrivenState<CounterState, CounterEvent>
{
    public CounterState() => On<CounterIncremented>(e => Count += e.By);

    [Id(0)] public int Count { get; set; }
}

internal enum DoorStatus { Closed, Open }
internal enum DoorCommand { Open, Close }

[GenerateSerializer]
internal abstract class DoorEvent(DoorCommand command) : StateMachineEvent<DoorCommand>(command);
[GenerateSerializer]
internal sealed class DoorOpened() : DoorEvent(DoorCommand.Open);
[GenerateSerializer]
internal sealed class DoorClosed() : DoorEvent(DoorCommand.Close);

[GenerateSerializer]
internal sealed class DoorState : EventDrivenStateMachineState<DoorState, DoorEvent, DoorStatus, DoorCommand>
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

internal sealed class SmokeTypeMapper : TypeMapper
{
    public SmokeTypeMapper() => AddType(typeof(CounterIncremented), "smoke.counter-incremented");
}

internal sealed class SmokeScope : IEventDrivenSessionScope<CounterState, CounterEvent>
{
    public ILogger Logger => NullLogger.Instance;

    public IEventDrivenSession<CounterState, CounterEvent> CreateSession() => new SmokeSession();

    public Task<Result<ISaveChangesResponse>> SaveChanges(IEventDrivenSession<CounterState, CounterEvent> session)
    {
        ISaveChangesResponse response = new SaveChangesResponse(session.UncommittedEvents.Select(e => (IChange)new Change(e.GetType().Name, e)).ToList(), session.UncommittedEvents.Count);
        return Task.FromResult(Result.Success(response));
    }
}

[JsonSerializable(typeof(SaveChangesResponse))]
[JsonSerializable(typeof(CounterIncremented))]
internal sealed partial class SmokeJsonContext : JsonSerializerContext;

internal sealed class SmokeSession : IEventDrivenSession<CounterState, CounterEvent>
{
    private readonly List<CounterEvent> _events = [];
    public CounterState State { get; } = new();
    public IReadOnlyList<CounterEvent> UncommittedEvents => _events;
    public void Apply<TEvent>(TEvent evt) where TEvent : CounterEvent { State.When(evt); _events.Add(evt); }
    public (CounterState PreviousState, CounterState CurrentState) Evolve<TEvent>(TEvent evt) where TEvent : CounterEvent { Apply(evt); return (State, State); }
    public void Dispose() { }
}
