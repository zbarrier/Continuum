# Continuum.AsyncExtensions

Async helpers for `Task`-based code.

## Features

- Tuple awaiters let you await two to twelve tasks concurrently and deconstruct their results in one statement.
  Each task can have a different result type. If any task fails, the first exception is thrown.

## Usage

```csharp
using Continuum.AsyncExtensions;

var (user, orders) = await (GetUserAsync(id), GetOrdersAsync(id));
```

## Parallel async iteration

Use the built-in `Parallel.ForEachAsync`. Setting `MaxDegreeOfParallelism` runs that many bodies concurrently
regardless of processor count, which suits I/O-bound work such as batched database writes:

```csharp
await Parallel.ForEachAsync(
    batches,
    new ParallelOptions { MaxDegreeOfParallelism = 20 },
    async (batch, ct) => await InsertBatchAsync(batch, ct));
```

- The default (`-1`) uses `Environment.ProcessorCount`, which can be 1 on small cloud instances.
- Use `int.MaxValue` for effectively unlimited concurrency; workers are only started as items are available.
- Keep the limit at or below your database connection pool size (100 by default for SqlClient) to avoid
  connection timeouts.
