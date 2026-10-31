# Continuum.NewId

Sequential, time-ordered, globally unique identifiers for .NET. A `NewId` is 128 bits, like a `Guid`, but it is generated from a timestamp, a worker id (the machine's MAC address or a host name hash), an optional process id, and a sequence number. Ids generated later sort after ids generated earlier, so they make good clustered index keys, and they can be generated on many machines without coordination.

## Install

```
dotnet add package Continuum.NewId
```

## Usage

```csharp
using Continuum;

NewId id = NewId.Next();

Guid sequential = NewId.NextSequentialGuid(); // PostgreSQL, MySQL, general use
Guid mssql = NewId.NextGuid();                // MSSQL Server uniqueidentifier ordering

DateTime created = id.Timestamp;              // UTC time the id was generated
```

### Guid ordering

MSSQL Server sorts `uniqueidentifier` values using a different byte order than most other systems, so `NewId` can produce either format:

| Method | Ordered correctly in | Convert back with |
|---|---|---|
| `NextSequentialGuid()` / `ToSequentialGuid()` | PostgreSQL, MySQL, string/byte comparison | `NewId.FromSequentialGuid(guid)` or `guid.ToNewIdFromSequential()` |
| `NextGuid()` / `ToGuid()` | MSSQL Server | `NewId.FromGuid(guid)` or `guid.ToNewId()` |

### Formatting

`ToString` accepts the same format specifiers as `Guid` (`D`, `N`, `B`, `P`). Add `S` (for example `"DS"`) to format the bytes in sequential order. Custom formatters are also included:

```csharp
id.ToString("N");
id.ToString(new ZBase32Formatter());     // 26 characters, human friendly
id.ToString(new Base32Formatter(true));  // 26 characters, upper case

NewId parsed = new ZBase32Parser().Parse(text);
```

### JSON

`NewIdConverter` reads and writes a `NewId` as a sequential `Guid` string with System.Text.Json:

```csharp
using Continuum.Converters.Json;

var options = new JsonSerializerOptions { Converters = { new NewIdConverter() } };
```

### Configuration

The default generator uses `BestPossibleWorkerIdProvider` (MAC address, falling back to a host name hash) and `DateTimeTickProvider`. To change them, call these methods at startup, before the first id is generated:

```csharp
NewId.SetProcessIdProvider(new CurrentProcessIdProvider()); // unique per process on the same machine
NewId.SetTickProvider(new StopwatchTickProvider());         // never goes backward; see below
```

`StopwatchTickProvider` was originally used for higher resolution timestamps, but on .NET 10 `DateTime.UtcNow` has about the same resolution. Its remaining benefit is that it never moves backward when the system clock is adjusted, because it adds elapsed `Stopwatch` time to the wall-clock time captured at startup. The trade-off is that its timestamps slowly drift from wall-clock time over long uptimes.

#### Containers

Containers often lack a MAC address that is unique across hosts. Before Docker Engine 28.0, a container's MAC address on bridge networks was derived from its internal IP address, so containers on different hosts that received the same internal IP got the same worker id and could generate duplicate ids. Docker Engine 28.0 and later assign random MAC addresses, but other container runtimes and network setups may still reuse addresses. The process id does not help, since it is often 1 inside a container. In container deployments, supply an `IWorkerIdProvider` that returns 6 bytes unique to each running process, such as a hash of the pod or container name, or of the Orleans silo address:

```csharp
NewId.SetWorkerIdProvider(new MyWorkerIdProvider());
```

## Orleans

To pass `NewId` values through Microsoft Orleans, install [Continuum.NewId.Orleans](https://www.nuget.org/packages/Continuum.NewId.Orleans).

## Attribution

Adapted from [NewId](https://github.com/phatboyg/NewId) by Chris Patterson, licensed under the Apache License 2.0. See NOTICE for details.
