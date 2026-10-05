# Continuum.Persistence.Orleans.KurrentDB

A [KurrentDB](https://www.kurrent.io/) (formerly EventStoreDB) grain storage provider

## Install

```
dotnet add package Continuum.Persistence.Orleans.KurrentDB
```

## Configure

The provider resolves a keyed `KurrentDBClient`, a keyed `IGrainStorageSerializer` and a keyed `ITypeMapper` using `ConnectionName`.

```csharp
siloBuilder.AddKurrentDBGrainStorageAsDefault(options =>
{
	options.ConnectionName = "kurrentdb";
});

// or a named provider
siloBuilder.AddKurrentDBGrainStorage("kurrent", options =>
{
	options.ConnectionName = "kurrentdb";
	options.MaxStateEventCount = 10;
});
```

| Option | Default | Description |
| --- | --- | --- |
| `ConnectionName` | (required) | Key used to resolve the client, serializer and type mapper. |
| `Credentials` | `UseDefault = true` | Connection default credentials, or an auth token / username and password. |
| `DeleteStateOnClear` | `false` | Delete the stream on clear instead of appending a clear marker. See [Clearing state](#clearing-state). |
| `MaxStateEventCount` | `5` | Events KurrentDB keeps per state stream. `null` keeps every event. See [Retention](#retention). |
| `ContentType` | inferred | Content type recorded on each event. `application/json` for the Continuum and Orleans JSON serializers, `application/octet-stream` otherwise. Set it for a custom JSON serializer. |
| `StreamNameFormatter` | `state__{GrainType}-{GrainKey}` | Stream name for a grain. |
| `EventIdGenerator` | `NewId.NextSequentialGuid` | Event id for each appended event. |
| `TypeMapKinds` | `Snapshot` | Which type map is used to name stored state events. |
| `InitStage` | `ApplicationServices` | Silo lifecycle stage the provider initializes in. |

Options are validated at silo startup.

## Stream names

By default a grain's state is stored in `state__{GrainType}-{GrainKey}`. The `state__` prefix keeps state streams apart from the event streams written by `Continuum.EventSourcing.Orleans.KurrentDB`. It sits before the first `-`, so state streams also have their own KurrentDB category. The default name does not include the ServiceId; services sharing one KurrentDB instance can use `StreamNameFormatter` to keep their streams apart.

## ETags and conflicts

The ETag is the stream revision of the latest state event. Writes and clears use it as the expected revision, so a stale ETag, or a missing ETag when the stream already exists, throws Orleans' `InconsistentStateException` with the stored and current ETags. Other KurrentDB failures are wrapped in `KurrentDBStorageException`.

## Clearing state

- **Clear marker (default):** clearing appends a marker event. The stream stays, its revisions keep increasing, and a read after the marker returns no state.
- **Delete (`DeleteStateOnClear = true`):** clearing soft-deletes the stream. The next write recreates it and continues from the next revision, so ETags never repeat.

Clearing a grain that has no state does nothing.

## Retention

KurrentDB keeps only the newest `MaxStateEventCount` events of each state stream, using the stream's `$maxCount` metadata. Older events are hidden from reads straight away and removed when KurrentDB scavenges. Only the latest event is used as the grain's state, so values above 1 just keep a short history for inspection. KurrentDB counts clear markers toward the limit.

The provider sets the limit when it creates a stream: on the first write for a grain, and on the first write after a delete-mode clear. Keep in mind:

- Changing `MaxStateEventCount` does not update existing streams. Change their metadata in KurrentDB if needed.
- The state is appended before the limit is set. If setting the limit fails, the write throws but the state is already saved, and that stream has no limit until its metadata is fixed in KurrentDB.

## Streaming pub/sub store

`AddKurrentDBPubSubStore` registers the provider as Orleans' `PubSubStore`, with the Orleans serializer and a type mapping for Orleans' internal pub/sub state type:

```csharp
siloBuilder.AddKurrentDBPubSubStore(options =>
{
	options.ConnectionName = "kurrentdb";
});
```

Pub/sub state can be rebuilt, and storing it ties the data to an internal Orleans type. Use `AddMemoryGrainStorage("PubSubStore")` instead if you would rather avoid that.

The serializer and type mapper are registered under `ConnectionName`, so the pub/sub store needs a connection name of its own, with its own keyed `KurrentDBClient`. Silo startup fails if another KurrentDB grain storage provider uses the same connection name, or if another serializer or type mapper is registered for it.

## Telemetry

The provider does not publish its own telemetry. Orleans already records storage metrics and activities for every grain storage provider.
