# Continuum.EventSourcing.Orleans

Shared [Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/) log-consistency support for Continuum event-sourced grains. Storage providers such as `Continuum.EventSourcing.Orleans.KurrentDB` and `Continuum.EventSourcing.Orleans.CosmosDB` build on it.

## Install

```
dotnet add package Continuum.EventSourcing.Orleans
```

## Contents

- `LogViewAdaptor` - the Orleans log view adaptor used by journaled grains. Events are appended to an `ILogConsistentStorage`, which is the source of truth. The latest view is kept as a snapshot in `IGrainStorage`, which is only a cache.
- `ILogConsistentStorage` - the event log abstraction that storage providers implement: read, read the most recent events, append with optimistic concurrency, and read back the transaction ID of an append.
- `ITransactionIdGenerator` - creates the transaction ID stored with every event of an append, so an append whose outcome is unknown can be recognized after a failure. Register your own implementation to replace the default (`Guid.CreateVersion7()`).
- `TenantLogConsistentStorage` - routes each grain to a tenant-specific or default `ILogConsistentStorage`.
- `EventSourcingTelemetry` - the activity source, meter and instrument names emitted by log-consistency storage providers.

Event metadata types are not in this package: `IEventMetadata` is in `Continuum.Core` and `EventMetadata` is in `Continuum.Core.Orleans`.

## Loading only the most recent events

A grain whose stream grows forever, such as a per-tenant number generator, can be marked with `[LoadMostRecentEventsOnly(count)]` (in `Continuum.Core.Orleans`). The adaptor then catches up from the log by reading only the grain's last `count` events, in one backwards read, instead of every event since its snapshot:

```csharp
[LoadMostRecentEventsOnly(100)]
public sealed class OrderNumberGeneratorGrain : EventDrivenGrain<OrderNumberGeneratorState, OrderNumberGeneratorEvent> { }
```

- Snapshots work as usual. A snapshot within the last `count` events is caught up from where it is. With no snapshot, or one further behind, the state is built from a new state and the last `count` events only.
- The grain's version is still the log's real version, so appends keep optimistic concurrency.
- The state must not depend on older events: anything it needs from them is lost when it is rebuilt. For the state to be the same however it was built, it should keep only what its last `count` events give it, trimming older data itself as new events are applied.
- Providers implement this through `ILogConsistentStorage.ReadMostRecentAsync`.
