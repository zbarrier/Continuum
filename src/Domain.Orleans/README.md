# Continuum.Domain.Orleans

[Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/) implementation of [Continuum.Domain](https://www.nuget.org/packages/Continuum.Domain): journaled event-driven grains, deep-copied stream transactions, multi-stream transactions, and [Stateless](https://github.com/dotnet-state-machine/stateless) state machines.

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
    public Task<Result<IStreamTransactionResponse>> Increment(int by)
        => Result.Success()
            .BindWithStreamTransaction(this, transaction => transaction.Apply(new CounterIncremented(by)));
}
```

Event handlers match the exact event type. The grain resolves `DeepCopier<TState>` and a keyed `ITypeMapper` (keyed by the `connectionName` constructor argument) from the service provider, and needs a log-consistency provider configured.

## State machine grain

```csharp
[GenerateSerializer]
public sealed class DoorState : EventDrivenStateMachineState<DoorState, DoorEvent, DoorStatus, DoorCommand>
{
    public DoorState() : base(DoorStatus.Closed) { }

    [Id(0)] public int OpenCount { get; set; }

    protected override void ConfigureEventHandlers() => On<DoorOpened>(_ => OpenCount++);

    protected override void ConfigureStateMachine()
    {
        StateMachine.Configure(DoorStatus.Closed).Permit(DoorCommand.Open, DoorStatus.Open);
        StateMachine.Configure(DoorStatus.Open).Permit(DoorCommand.Close, DoorStatus.Closed);
    }
}
```

Each event carries the command it fires. The current status is stored on the state, and the constructor builds the state machine over it and calls `ConfigureStateMachine`, so copies and deserialized states (including snapshots) always have their transitions.

## Transactions

`BindWithStreamTransaction` begins a stream transaction over a deep copy of the grain state, runs the callback, and saves the transaction. The grain's state only changes when the save succeeds, so a failed command leaves the grain unchanged without any rollback. Exceptions thrown by the callback or the save are logged through the grain's `Logger` and returned as failures.

`Apply` and `Evolve` on the transaction always apply events to the grain's own stream. To change other grains in the same atomic append (KurrentDB only), call `PrepareAsync` on the transaction:

```csharp
public Task<Result<IStreamTransactionResponse>> TransferAsync(string toAccountId, decimal amount)
    => Result.Success()
        .BindWithStreamTransaction(this, async transaction =>
        {
            transaction.Apply(new MoneyDebited(toAccountId, amount));
            var destination = GrainFactory.GetGrain<IAccountGrain>(toAccountId).AsReference<ITransactionParticipant>();
            return await transaction.PrepareAsync(destination, new CreditAccount(amount));
        });
```

A participant opts commands in with `OnTransaction<TCommand>(handler)`. The handler receives an `IStreamTransaction`, applies events without saving them, and those events are returned to the coordinator. Use the same handler from the participant's own command method so a direct call and a prepared call share their business logic.

Every saved event carries the normal transaction metadata. Multi-stream metadata and participant commit or rollback calls only happen when participants were prepared. When the coordinator prepares participants, it must apply at least one event to its own stream.

## Native AOT

The library is AOT-compatible, but Orleans itself doesn't yet support Native AOT at runtime.
