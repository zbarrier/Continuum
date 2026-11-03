# Continuum.EventSourcing.Orleans

Shared [Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/) log-consistency support for Continuum event-sourced grains. Storage providers such as `Continuum.EventSourcing.Orleans.KurrentDB` and `Continuum.EventSourcing.Orleans.CosmosDB` build on it.

## Install

```
dotnet add package Continuum.EventSourcing.Orleans
```

## Contents

- `LogViewAdaptor` - the Orleans log view adaptor used by journaled grains. Events are appended to an `ILogConsistentStorage`, which is the source of truth. The latest view is kept as a snapshot in `IGrainStorage`, which is only a cache.
- `ILogConsistentStorage` - the event log abstraction that storage providers implement: read, append with optimistic concurrency, and read back the transaction ID of an append.
- `ITransactionIdGenerator` - creates the transaction ID stored with every event of an append, so an append whose outcome is unknown can be recognized after a failure. Register your own implementation to replace the default (`Guid.CreateVersion7()`).
- `TenantLogConsistentStorage` - routes each grain to a tenant-specific or default `ILogConsistentStorage`.
- `EventSourcingTelemetry` - the activity source, meter and instrument names emitted by log-consistency storage providers.

Event metadata types are not in this package: `IEventMetadata` is in `Continuum.Core` and `EventMetadata` is in `Continuum.Core.Orleans`.
