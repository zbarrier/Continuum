# Continuum.EventSourcing.Orleans.KurrentDB.Abstractions

Shared [KurrentDB](https://www.kurrent.io/) event metadata contracts used by Continuum's KurrentDB packages.

## Install

```
dotnet add package Continuum.EventSourcing.Orleans.KurrentDB.Abstractions
```

## KurrentDBEventMetadata

The KurrentDB implementation of `IEventMetadata`. Names beginning with `$` are reserved by KurrentDB and are dropped from the metadata bag.

```csharp
var metadata = KurrentDBEventMetadata.Create(new Dictionary<string, string?>
{
    ["tenant"] = "contoso",
});
```

## KurrentDBEventMetadataCodec

Reads and writes the JSON document KurrentDB stores in an event's metadata slot.

```csharp
// Appending: tracing is injected by the KurrentDB client, so it is not written here.
byte[]? bytes = KurrentDBEventMetadataCodec.Write(metadata, transaction);

// Reading: "$traceId", "$spanId" and the transaction entries are lifted onto typed properties.
KurrentDBEventMetadata read = KurrentDBEventMetadataCodec.Read(resolvedEvent.Event.Metadata.Span);
```

Unreadable metadata (absent, empty, or not a JSON object) yields `KurrentDBEventMetadata.Empty` rather than throwing, so one foreign event cannot stall a consumer.
