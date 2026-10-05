# Continuum.EventSourcing.Orleans

[Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/) implementation of [Continuum.EventSourcing](https://www.nuget.org/packages/Continuum.EventSourcing).

## Install

```
dotnet add package Continuum.EventSourcing.Orleans
```

## Event metadata

`EventMetadata` is the Orleans serializable, immutable base implementation of `IEventMetadata`. Transports derive from it to supply their own factory methods and reserved names, for example `KurrentDBEventMetadata` in `Continuum.EventSourcing.Orleans.KurrentDB.Abstractions`.

Because it is marked `[Immutable]`, Orleans passes instances between grains without deep copying them.
