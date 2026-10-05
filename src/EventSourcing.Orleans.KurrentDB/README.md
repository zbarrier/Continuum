# Continuum.EventSourcing.Orleans.KurrentDB

A [KurrentDB](https://www.kurrent.io/) log-consistency provider for [Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/) journaled grains. Each grain's events are appended to its own KurrentDB stream, and the latest view is snapshotted to a standard Orleans grain storage provider.

## Install

```
dotnet add package Continuum.EventSourcing.Orleans.KurrentDB
```

## Configure

The provider resolves a keyed `KurrentDBClient`, a keyed `IGrainStorageSerializer` and a keyed `ITypeMapper` using `ConnectionName`.

```csharp
siloBuilder.AddKurrentDBBasedLogConsistencyProviderAsDefault(options =>
{
    options.ConnectionName = "kurrentdb";
});
```

Options can also be bound from configuration under `Orleans:EventSourcing:KurrentDB:{providerName}`.

| Option | Default | Description |
| --- | --- | --- |
| `ConnectionName` | (required) | Key used to resolve the client, serializer and type mapper. |
| `Credentials` | `UseDefault = true` | Connection default credentials, or an auth token / username and password. |
| `StreamNameFormatter` | `{GrainType}-{GrainKey}` | Stream name for a grain. The grain type becomes the KurrentDB category. |
| `EventIdGenerator` | `NewId.NextSequentialGuid` | Event id for each appended event. |
| `TypeMapKinds` | `DomainEvent` | Which type map is used to name stored events. |
| `InitStage` | `ApplicationServices` | Silo lifecycle stage the provider initializes in. |

## Stream names and categories

By default each grain's stream is named `{GrainType}-{GrainKey}`. KurrentDB takes a stream's category from the text before the first `-`, so the grain type becomes the category and `$ce-{GrainType}` subscriptions see every instance of that grain.

A grain type that itself contains `-`, such as `[GrainType("snack-grain")]`, would be filed under the truncated category `snack`. To catch this:

- The package includes analyzer `CKDB001`, which fails the build when a journaled grain declares a grain type containing `-`.
- At silo startup the provider rejects journaled grains it stores whose grain type contains `-`.

Remove the `-` from the grain type. If a journaled grain with `-` in its grain type is stored by a different log-consistency provider, suppress `CKDB001` for that grain with `[SuppressMessage("Continuum.EventSourcing.KurrentDB", "CKDB001")]`, or lower the rule in `.editorconfig` with `dotnet_diagnostic.CKDB001.severity = none`.

`StreamNameFormatter` is for adding identity to the stream name, such as a tenant: `{GrainType}-{TenantId}_{GrainKey}`. A custom formatter must keep the grain type before the first `-` so it remains the category. It is not a way around the grain type rule above. The default name does not include the ServiceId, so services sharing one KurrentDB instance can use a formatter to keep their streams apart in the same way.

## Grain storage

A grain storage provider is required, even when snapshots are not used. Most applications do not need snapshots and register the in-memory provider:

```csharp
siloBuilder.AddMemoryGrainStorageAsDefault();
```

## Versions

Versions are event counts, as in Orleans: version 0 means no events and version n means n events have been written. KurrentDB stream revisions are zero-based, so the event at revision r produces version r + 1. The provider converts between the two internally; callers only ever see event counts.

## Transaction metadata

regroup events committed together. See `Continuum.EventSourcing.Orleans.KurrentDB.Abstractions`.

## Telemetry

The provider publishes metrics through `System.Diagnostics.Metrics` and traces through `ActivitySource`, so no OpenTelemetry package is required. Register the names with OpenTelemetry (for example in Aspire service defaults):

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddMeter(EventSourcingTelemetry.MeterName))
    .WithTracing(tracing => tracing.AddSource(EventSourcingTelemetry.ActivitySourceName));
```

| Metric | Unit | Description |
| --- | --- | --- |
| `continuum.eventsourcing.operation.duration` | `s` | Duration of read, get-last-version and append operations. |
| `continuum.eventsourcing.events` | `{event}` | Events read or appended. |

Metrics are tagged with `db.system.name`, `db.operation.name`, `continuum.provider.name`, `continuum.grain.type` and, on failure, `error.type`. Each operation also produces a span that additionally carries the grain id, stream name and event count.
