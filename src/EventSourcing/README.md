# Continuum.EventSourcing

Host-independent event sourcing abstractions for Continuum.

## Install

```
dotnet add package Continuum.EventSourcing
```

## Event metadata

`IEventMetadata` is a read-only, string keyed bag of producer supplied values describing a streamed event, plus typed properties lifted out of the same document by the transport:

- `TraceId` / `SpanId` - the W3C trace context the event was produced within.
- `TransactionId`, `TransactionSize`, `TransactionPartitionSize`, `TransactionPartitionIndex` - the append the event was committed in, so a consumer can regroup events written together.

Metadata values are plain strings and are never interpreted. Structured data belongs in the event payload, where the type mapper and serializer resolve types properly. Names beginning with a transport's reserved prefix (for KurrentDB, `$`) are owned by the transport and are dropped from the bag.

Transport specific implementations live in separate packages, such as `Continuum.EventSourcing.Orleans.KurrentDB.Abstractions`.
