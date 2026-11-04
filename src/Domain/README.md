# Continuum.Domain

Host-independent domain abstractions for event-driven state, stream transactions, and state machines.

This package contains contracts and `Result` pipeline extensions only. Hosts provide the implementations; [Continuum.Domain.Orleans](https://www.nuget.org/packages/Continuum.Domain.Orleans) is the Microsoft Orleans implementation.

## Install

```
dotnet add package Continuum.Domain
```

## Concepts

| Type | Purpose |
|---|---|
| `IEventDrivenState<TState, TEventBase>` | State rebuilt by applying events. |
| `IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>` | Event-driven state whose events fire state machine commands. |
| `IStreamTransaction<TState, TEventBase>` | Works on an isolated copy of the state and tracks uncommitted events. |
| `IStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>` | The state machine equivalent of `IStreamTransaction`. |
| `IStreamTransactionResponse` | The saved changes (event type name and event) and the new version. |

## Stream transactions

A stream transaction applies events to a copy of the state. The owner's state only changes when the transaction is saved, so a failed command never needs to roll back. Hosts provide the API that begins and saves transactions; for Orleans grains, use `BindWithStreamTransaction` from `Continuum.Domain.Orleans`.
