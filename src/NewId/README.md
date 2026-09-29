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
var options = new JsonSerializerOptions { Converters = { new NewIdConverter() } };
```

### Configuration

The default generator uses `BestPossibleWorkerIdProvider` (MAC address, falling back to a host name hash) and `DateTimeTickProvider`. To change them, call these methods at startup, before the first id is generated:

```csharp
NewId.SetProcessIdProvider(new CurrentProcessIdProvider()); // unique per process on the same machine
NewId.SetTickProvider(new StopwatchTickProvider());         // higher resolution timestamps
```

## Orleans

To pass `NewId` values through Microsoft Orleans, install [Continuum.NewId.Orleans](../NewId.Orleans/README.md).

## Attribution

Adapted from [NewId](https://github.com/phatboyg/NewId) by Chris Patterson, licensed under the Apache License 2.0. See NOTICE for details.
