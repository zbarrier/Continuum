# Continuum.Domain

Host-independent domain abstractions for event-driven state, transactional sessions, and state machines.

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
| `IEventDrivenSession<TState, TEventBase>` | Works on an isolated copy of the state and tracks uncommitted events. |
| `IEventDrivenSessionScope<TState, TEventBase>` | Creates sessions and saves their uncommitted events. |
| `ISaveChangesResponse` | The saved changes (event type name and event) and the new version. |

## Transactional sessions

A session applies events to a copy of the state. The owner's state only changes when `SaveChanges` succeeds, so a failed command never needs to roll back:

```csharp
using var session = scope.CreateSession();
session.Apply(new OrderPlaced(orderId));
Result<ISaveChangesResponse> result = await scope.SaveChanges(session);
```

## Result pipelines

The `WithSessionScope`, `BindWithSessionScope`, `WithStateMachineSessionScope`, and `BindWithStateMachineSessionScope` extensions create a session, run a command against it, and save it as part of a `Result` pipeline. Exceptions are logged through the scope's `ILogger` and returned as failures.
