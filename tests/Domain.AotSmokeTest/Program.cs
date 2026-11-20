using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

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
var closePermitted = door.StateMachine.CanFire(DoorCommand.Close);
door.When(new DoorClosed());
Check("State machine status", closePermitted && door.Status == DoorStatus.Closed && door.StateMachine.CanFire(DoorCommand.Open) && door.OpenCount == 1);

// StreamTransactionResponseConverter with source-generated JSON metadata
var mapper = new SmokeTypeMapper();
var options = new JsonSerializerOptions { TypeInfoResolver = SmokeJsonContext.Default };
options.Converters.Add(new StreamTransactionResponseConverter(mapper));
var responseInfo = (JsonTypeInfo<StreamTransactionResponse>)options.GetTypeInfo(typeof(StreamTransactionResponse));
var json = JsonSerializer.Serialize(new StreamTransactionResponse([new Change("smoke.counter-incremented", new CounterIncremented(7))], 4), responseInfo);
var back = JsonSerializer.Deserialize(json, responseInfo);
Check("StreamTransactionResponse round-trip", back is { Version: 4 } && back.Changes.Single().Event is CounterIncremented { By: 7 });

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
internal abstract class DoorEvent;
[GenerateSerializer]
internal sealed class DoorOpened : DoorEvent;
[GenerateSerializer]
internal sealed class DoorClosed : DoorEvent;

[GenerateSerializer]
internal sealed class DoorState : EventDrivenStateMachineState<DoorState, DoorEvent, DoorStatus, DoorCommand>
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

internal sealed class SmokeTypeMapper : TypeMapper
{
    public SmokeTypeMapper() => AddType(typeof(CounterIncremented), "smoke.counter-incremented");
}

[JsonSerializable(typeof(StreamTransactionResponse))]
[JsonSerializable(typeof(CounterIncremented))]
internal sealed partial class SmokeJsonContext : JsonSerializerContext;
