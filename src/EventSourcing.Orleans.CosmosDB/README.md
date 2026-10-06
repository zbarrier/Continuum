# Continuum.EventSourcing.Orleans.CosmosDB

An [Azure Cosmos DB](https://learn.microsoft.com/azure/cosmos-db/) log-consistency provider for [Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/) journaled grains. Each grain's events are stored as items in a Cosmos DB container, and the latest view is snapshotted to a standard Orleans grain storage provider.

The storage design is based on [Eveneum](https://github.com/Eveneum/Eveneum) (MIT).

## Install

```
dotnet add package Continuum.EventSourcing.Orleans.CosmosDB
```

## Configure

The provider resolves a keyed `CosmosClient`, a keyed `IGrainStorageSerializer` and a keyed `ITypeMapper` using `ConnectionName`. The serializer must produce JSON (for example `TypeMappedJsonGrainStorageSerializer`); event payloads are embedded as JSON in the Cosmos DB items. Item envelopes are serialized with source-generated System.Text.Json, so the provider is trimming and Native AOT compatible.

```csharp
siloBuilder.AddAzureCosmosDBBasedLogConsistencyProviderAsDefault(options =>
{
    options.ConnectionName = "cosmosdb";
    options.DatabaseName = "orleans";
    options.ContainerName = "events";
});
```

The database and container are not created by the provider. Create the container with the partition key path `/streamName`; each grain's events and stream header are stored in that grain's partition so appends can be written in a single transactional batch.

<!-- TODO: Once the examples exist, document the recommended container setup (indexing policy, infrastructure-as-code snippet) and link to the Aspire local-development example. -->

Options can also be bound from configuration

| Option | Default | Description |
| --- | --- | --- |
| `ConnectionName` | (required) | Key used to resolve the client, serializer and type mapper. |
| `DatabaseName` | (required) | Cosmos DB database holding the event container. |
| `ContainerName` | (required) | Cosmos DB container events are written to. |
| `BatchSize` | `100` | Maximum operations per transactional batch (2-100). An append is written atomically with the stream header, so at most `BatchSize - 1` events can be appended per write. |
| `QueryMaxItemCount` | `1000` | Maximum items returned per query page. |
| `IgnoreMissingTypes` | `false` | Skip events whose type can no longer be resolved instead of failing. |
| `StreamNameFormatter` | `{ServiceId}/{GrainId}` | Stream name (partition) for a grain. |
| `EventIdGenerator` | `NewId.Next` | Event id for each appended event. |
| `TypeMapKinds` | `DomainEvent \| Metadata` | Which type map is used to name stored events. |
| `StartupConnectionTimeout` | `00:00:30` | How long silo startup retries when Cosmos DB is unreachable while validating the container. |
| `RequestChargeWarningThreshold` | `50` | Operations consuming more request units (RUs) than this are logged as warnings; others are logged at debug level. |

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
| `continuum.eventsourcing.cosmosdb.request_charge` | `{request_unit}` | Request units consumed per operation. |

Metrics are tagged with `db.system.name`, `db.operation.name`, `continuum.provider.name`, `continuum.grain.type` and, on failure, `error.type`. Each operation also produces a span that additionally carries the grain id, stream name, event count and request charge; Cosmos DB SDK spans appear as its children.
| `InitStage` | `ApplicationServices` | Silo lifecycle stage the provider initializes in. |
