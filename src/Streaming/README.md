# Continuum.Streaming

Provider-neutral abstractions for consuming event streams in Continuum. Storage- and host-specific implementations (for example the Orleans and KurrentDB streaming packages) build on these types.

## Install

```
dotnet add package Continuum.Streaming
```

## Abstractions

| Type | Purpose |
| --- | --- |
| `IStreamedEvent<T>` | An event delivered from a stream, with its id, type, stream name, key, topic, partition, version, position, sequence number, timestamp and metadata. |
| `IStreamedEventMetadata` | Read-only string metadata attached to a streamed event, with typed accessors for tracing and transaction information. Names starting with `$` are reserved. |
| `StreamedName` | A stream name split into its topic and stream key. |
| `IStreamedNameParser` | Parses a provider stream name into a `StreamedName`. |
| `ICheckpointStore<TStreamPosition>` | Loads and stores the last processed position of a named subscription. |
| `IStreamSubscription` / `IHostedStreamSubscription` | A named subscription and one whose lifetime is managed by a host. |

## Register a stream name parser

Parsers are registered as keyed services so each connection can use its own naming convention:

```csharp
builder.AddStreamedNameParser<MyStreamNameParser>("orders");
```
