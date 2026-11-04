# Multi-stream transactions

Status: draft. Captures the design as discussed so far; open questions are listed at the end and must be settled before implementation.

## Goal

Let one command append events to several grains' streams atomically, so either every grain's events are committed or none are.

## Provider support

- **KurrentDB only.** KurrentDB 26.1+ can append records to multiple streams in one call (`appendRecords`), with an expected version per stream. The KurrentDB log-consistency provider uses it to commit a transaction.
- **Cosmos DB is not supported.** A Cosmos transactional batch is limited to one partition key, and each grain stream is its own partition.

## Shape

### Coordinating grain

- Uses `BindWithStreamTransaction`, the same Result extension used for single-stream commands. Its callback receives an `ICoordinatingStreamTransaction`: `Apply`/`Evolve` change the coordinator's own stream, and `PrepareAsync` enlists participants. With no participants it is a normal single-stream save.
- Calls `PrepareAsync` on each secondary grain, collects the prepared events, groups them per stream, and has the KurrentDB log provider commit them in a single `appendRecords` call alongside its own events.
- On success, calls `CommitAsync` on each secondary grain. On failure, calls `RollbackAsync`.
- **Always writes at least one event.** If a command would not produce an event on the coordinator's own stream, a different grain should coordinate it.
- **New append route.** The log provider needs a coordinated-transaction route that takes the grouped per-stream events and expected versions and calls `appendRecords` instead of the single-stream append.
- **Ambiguous outcomes use the existing check.** The append is atomic, so the coordinator's own stream commits with every other stream. Reading back the transaction ID on its own stream with `GetTransactionIdAsync`, as single-stream appends already do, tells it whether the whole transaction committed.
- **A concurrency conflict starts over.** On a version conflict on any stream, the coordinator refreshes its state from the new events and runs the command again from the beginning, as if it had just been called. Its business checks run against the new state, and it calls `PrepareAsync` on every secondary grain again, so they also re-run their checks against their current state.

### Secondary grains

- **Opt-in.** A grain must explicitly allow a command to take part in a coordinated transaction, using a command mapper similar to the event-to-handler mapping. Nothing joins a transaction automatically.
- **Core method pattern (only for opted-in commands).** A command that supports transactions puts its logic in a core method that takes the command and an `IStreamTransaction` and applies events without saving. The normal command method calls the core method through `BindWithStreamTransaction`, so a normal call can always expect its events to be committed. `OnTransaction<TCommand>(core)` registers the same core method for `PrepareAsync`, which runs it on a framework-created `IStreamTransaction` and returns the events instead of saving them. Participants never receive the coordinating transaction, so they cannot start nested transactions. Commands that do not support transactions are written as usual, and preparing them throws `NotSupportedException`.
- **Interleaving (convention).** Queries may use `[ReadOnly]` or `[AlwaysInterleave]`; command methods should not. A prepare never changes the grain's state, so an interleaved query only sees the pre-commit state, and a command that commits during a prepare makes the expected version fail and the coordinator retry.
- **`PrepareAsync`**
- **Events are optional.** A secondary grain may return no events.
- **Expected version is usually required, but optional.** Most secondary grains return an expected version, and the append checks it, so the transaction only commits if that grain's stream has not changed since it prepared. A grain may instead ask for no version check. That must be an explicit "any version" value, not `0`: versions are event counts, so `0` already means the stream has no events. A grain that returns no events and no version check has nothing to add to the append.
- **No waiting.** A secondary grain does not block other commands while it holds a prepared transaction. Optimistic concurrency covers it: if another command changes its stream first, the expected version no longer matches, the append fails, and the coordinator starts over. An opt-in lock could be added later if retries on busy grains become a problem.
- The grain holds the prepared transaction's information until it is resolved.
- **`CommitAsync`** tells the grain the append succeeded. The grain discards the prepared transaction and refreshes from its stream (`RefreshNow`), which reads only the events after its confirmed version. It does not apply the prepared events in memory: the refresh is one small read, works when the append had no version check, and keeps every resolution path identical.
- **`RollbackAsync`** tells the grain to discard the prepared transaction.
- **`CommitAsync` and `RollbackAsync` are one-way (`[OneWay]`).** The coordinator has nothing to do with their result, and a lost call is covered by the commit notification and the timer. `PrepareAsync` stays a normal call, since the coordinator needs its result and any business error.
- **Idempotent resolution.** `CommitAsync`, the notification and the timer each refresh only if the stream is ahead of the confirmed version, so whichever arrives first does the read and the rest only discard the prepared data.
- **Read-your-writes on secondary grains is not guaranteed.** A query that reaches a secondary grain before it handles `CommitAsync` may see the previous state. See [Reading committed events](#reading-committed-events) for the opt-in that avoids it.
- **Timeout.** A timer fires when neither `CommitAsync` nor `RollbackAsync` arrives. Before discarding the prepared transaction, the grain checks the version of the last event in its stream. If the stream is ahead of its confirmed version, the transaction may have committed without `CommitAsync` or the notification reaching it, so it refreshes. Because each path only checks the stream, `CommitAsync`, the notification and the timer can arrive in any order.

### Timeouts

- The coordinator's transaction timeout and the secondary grains' timers are tied together: the coordinator must give up before any secondary grain's timer fires.
- The coordinator passes its remaining time to `PrepareAsync`, and each secondary grain sets its timer to that time plus a buffer. Passing a duration rather than an absolute deadline avoids relying on clocks agreeing across silos.
- The coordinator must not start the append after its own timeout has passed.
- Possible extra buffer for the commit notification to arrive before the timer fires (not decided). The timer checks the stream anyway, so this buffer would only save a stream read, not affect correctness.

### Commit notification

- Grains have an implicit subscription to a non-replayable stream that announces successful transactions. Delivery is expected in under a second.
- It covers the case where `CommitAsync` was never delivered: the grain refreshes from its stream when it hears the transaction committed.

## Reading committed events

Behavior grains keep internal state only for command logic. Queries on them return a read model or DTO built from events, never the internal state, and should stay small; heavy reads belong in projections. A query never refreshes or otherwise changes the grain.

- **Confirmed event cache (opt-in per grain type).** A `[KeepConfirmedEvents]` attribute on the grain class turns it on; it is off by default. Grain types whose streams can grow large (roughly 1,000+ events) should leave it off. The attribute is read once per grain type with reflection, cached in a static dictionary, and copied into a field at construction, so queries only read a `bool`.
- **The cache lives on the grain, never on the state.** It is not serialized, not in snapshots, and not deep-copied into stream transactions. A `ConfirmedEventCache<TEventBase>` component (a no-op instance when the attribute is absent) is filled from the existing `TransitionState` override, which only sees events applied to the confirmed state: activation replay, the grain's own appends, and refreshes. Prepared events and events read for a query never reach it.
- **Snapshots.** Activation does not fill the cache with a full read, which would defeat the snapshot. When a grain activates from a snapshot, the cache holds the snapshot's version as its base version plus the events after it. A read model on such a grain is built from the state plus the remaining events. Building from events alone is preferred where possible; using the state couples the read model to internal command state, so it is not the default.
- **Reading the stream.** `JournaledGrain.RetrieveConfirmedEvents` rejects a `toVersion` above the grain's confirmed version (verified in Orleans 10.4), and its log adaptor is internal. A protected helper reads committed events through `ILogConsistentStorage.ReadAsync` instead, which reads to the end of the stream.
- **Decision method on `EventDrivenGrain`** (and `EventDrivenStateMachineGrain`), with a `requireLatest` flag for queries that cannot tolerate stale data:

| Cache | Prepared transaction and `requireLatest` | Events returned |
|---|---|---|
| Off (default) | Either | The whole stream read from storage, which is always current |
| On | No | The cached events, with no storage read |
| On | Yes | The cached events plus the events after the confirmed version read from storage; the extra events are not added to the cache |

## Carrying transaction data through the log view adaptor

Secondary appends are not wrapped in a domain event. Orleans passes every submitted event to `UpdateView` for tentative state, then to the confirmed view, then to storage, so a wrapper event would have to be unwrapped on every one of those paths.

Instead, the data travels on the pending submission entry:

- `LogViewAdaptor` uses a submission entry type derived from `SubmissionEntry<TLogEntry>` with an extra transaction field. Single-stream saves leave it `null`.
- The coordinator sets a pending transaction context on the adaptor, raises its own events as conditional events, and clears the context in a `finally`. `MakeSubmissionEntry` stamps the context onto each entry it creates. This is safe because submission runs synchronously in one grain turn.
- Only the coordinator's own events reach `UpdateView` and storage as coordinator events.
- `WriteAsync` reads the batch with `GetCurrentBatchOfUpdates()`. If an entry carries a transaction, it writes up to and including that transaction's entries through `IMultiStreamLogConsistentStorage.AppendAsync` and returns that count. Any normal events queued before it join the same atomic append. The rest stay pending for the next write.
- Retries reuse the payload because it stays on the pending entries until Orleans removes the written range. Stale conditional entries are dropped with their payload, so a conflicting transaction fails cleanly.
- The coordinator always has at least one event, so the payload always has an entry to ride on.
- Ambiguous outcomes are resolved on the coordinator's stream with `GetTransactionIdAsync`, since the same transaction ID is written to every stream in the append.

## Existing building blocks

- Event metadata already records the transaction ID, the total event count, the event count for the event's own stream, and the event's index. Single-stream appends currently set both counts to the same value, so the format can already describe an append spanning streams.
- `ILogConsistentStorage.GetTransactionIdAsync` reads back the transaction ID at a version, which lets a writer find out whether an append that threw was actually committed.

## Storage layer (implemented)

- `IMultiStreamLogConsistentStorage` (EventSourcing.Orleans) extends `ILogConsistentStorage` with `AppendAsync(IReadOnlyList<LogAppend<TLogEntry>>, Guid transactionId)`. It is store-agnostic; grains and the coordinator depend only on it, never on KurrentDB types.
- `LogAppend<TLogEntry>` is one grain's part: grain type, grain ID, entries (may be empty), and a nullable expected version. `null` means no version check; `0` means the log must not exist. An empty entry list with an expected version is a check-only participant. At least one append must have entries, and a grain may appear only once.
- A version conflict on any log throws `LogAppendConflictException` (an `InconsistentStateException`) listing each `LogAppendConflict` (grain, expected version, actual version). Nothing is appended. A missing log reports actual version 0.
- The KurrentDB implementation maps this to `AppendRecordsAsync` (KurrentDB.Client 1.6.0, server 26.1+) with one `AppendRecord` per entry and one `StreamStateCheck` per expected version, and translates `AppendConsistencyViolationException` to `LogAppendConflictException`. Transaction metadata uses the total event count as the transaction size and each stream's event count as its partition size.
- A transaction cannot span tenants. `TenantLogConsistentStorage` routes a multi-stream append to the shared tenant's storage and throws `ArgumentException` when the grains belong to different tenants (a grain without a tenant counts as its own group).
- Verified against KurrentDB 26.1: atomic commit across streams, full rollback when any check fails, check-only streams, and per-stream metadata.

## Open questions

- Is `PrepareAsync`/`CommitAsync`/`RollbackAsync` the right model? It was considered, not confirmed.
- Are secondary grains always on the same KurrentDB connection as the coordinator? A transaction cannot span connections.
- How do subscribers and projections handle a transaction spanning several streams without serving a half-applied state, given the rule that events with an active cursor are never dropped?
- What publishes the commit notification, and on which stream provider?
