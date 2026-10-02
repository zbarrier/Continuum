# Continuum.Domain.Orleans

[Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/) implementation of [Continuum.Domain](https://www.nuget.org/packages/Continuum.Domain): journaled event-driven grains, deep-copied transactional sessions, and [Stateless](https://github.com/dotnet-state-machine/stateless) state machines.

## Install

```
dotnet add package Continuum.Domain.Orleans
```

## Event-driven grain

```csharp
[GenerateSerializer]
public abstract record CounterEvent;

[GenerateSerializer]
public sealed record CounterIncremented([property: Id(0)] int By) : CounterEvent;

[GenerateSerializer]
public sealed class CounterState : EventDrivenState<CounterState, CounterEvent>
{
    public CounterState() => On<CounterIncremented>(e => Count += e.By);

    [Id(0)] public int Count { get; set; }
}

public sealed class CounterGrain(IServiceProvider services)
    : EventDrivenGrain<CounterState, CounterEvent>(services, "counters"), ICounterGrain
{
    public async Task<Result<ISaveChangesResponse>> Increment(int by)
    {
        using var session = CreateSession();
        session.Apply(new CounterIncremented(by));
        return await SaveChanges(session);
    }
}
```

Event handlers match the exact event type. The grain resolves `DeepCopier<TState>` and a keyed `ITypeMapper` (keyed by the `connectionName` constructor argument) from the service provider, and needs a log-consistency provider configured.

## State machine grain

```csharp
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
```

Each event carries the command it fires. `ConfigureStateMachine` runs again on every session copy, because deep copies don't carry over Stateless transitions.

## Transactions

Sessions work on a deep copy of the grain state. The grain's state only changes when `SaveChanges` successfully raises the events, so a failed command leaves the grain unchanged without any rollback.

## Native AOT

The library is AOT-compatible, but Orleans itself doesn't yet support Native AOT at runtime.
