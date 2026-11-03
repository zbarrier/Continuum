# Multi-stream transactions

Status: draft. Captures the design as discussed so far; open questions are listed at the end and must be settled before implementation.

## Goal

Let one command append events to several grains' streams atomically, so either every grain's events are committed or none are.

## Provider support

- **KurrentDB only.** KurrentDB 26.1+ can append records to multiple streams in one call (`appendRecords`), with an expected version per stream. The KurrentDB log-consistency provider uses it to commit a transaction.
- **Cosmos DB is not supported.** A Cosmos transactional batch is limited to one partition key, and each grain stream is its own partition.

## Shape

### Coordinating grain

- Uses a new Result extension, `BindWithTransaction`, in place of the existing session-scoped binding (`BindWithSessionScope`).
- Calls `PrepareAsync` on each secondary grain, collects the prepared events, groups them per stream, and has the KurrentDB log provider commit them in a single `appendRecords` call alongside its own events.
- On success, calls `CommitAsync` on each secondary grain. On failure, calls `RollbackAsync`.
- **Always writes at least one event.** If a command would not produce an event on the coordinator's own stream, a different grain should coordinate it.
- **New append route.** The log provider needs a coordinated-transaction route that takes the grouped per-stream events and expected versions and calls `appendRecords` instead of the single-stream append.
- **Ambiguous outcomes use the existing check.** The append is atomic, so the coordinator's own stream commits with every other stream. Reading back the transaction ID on its own stream with `GetTransactionIdAsync`, as single-stream appends already do, tells it whether the whole transaction committed.
- **A concurrency conflict starts over.** On a version conflict on any stream, the coordinator refreshes its state from the new events and runs the command again from the beginning, as if it had just been called. Its business checks run against the new state, and it calls `PrepareAsync` on every secondary grain again, so they also re-run their checks against their current state.

### Secondary grains

- **Opt-in.** A grain must explicitly allow a command to take part in a coordinated transaction, using a command mapper similar to the event-to-handler mapping. Nothing joins a transaction automatically.
- **`PrepareAsync`** executes the command as it normally would in a session, but does not save the events. It returns the events and the information needed to append them, such as the expected version.
- **Events are optional.** A secondary grain may return no events.
- **Expected version is usually required, but optional.** Most secondary grains return an expected version, and the append checks it, so the transaction only commits if that grain's stream has not changed since it prepared. A grain may instead ask for no version check. That must be an explicit "any version" value, not `0`: versions are event counts, so `0` already means the stream has no events. A grain that returns no events and no version check has nothing to add to the append.
- **No waiting.** A secondary grain does not block other commands while it holds a prepared transaction. Optimistic concurrency covers it: if another command changes its stream first, the expected version no longer matches, the append fails, and the coordinator starts over. An opt-in lock could be added later if retries on busy grains become a problem.
- The grain holds the prepared transaction's information until it is resolved.
- **`CommitAsync`** tells the grain the append succeeded.
- **`RollbackAsync`** tells the grain to discard the prepared transaction.
- **Timeout.** A timer fires when neither `CommitAsync` nor `RollbackAsync` arrives. Before discarding the prepared transaction, the grain checks the version of the last event in its stream. If the stream is ahead of its confirmed version, the transaction may have committed without `CommitAsync` or the notification reaching it, so it refreshes. Because each path only checks the stream, `CommitAsync`, the notification and the timer can arrive in any order.

### Timeouts

- The coordinator's transaction timeout and the secondary grains' timers are tied together: the coordinator must give up before any secondary grain's timer fires.
- The coordinator passes its remaining time to `PrepareAsync`, and each secondary grain sets its timer to that time plus a buffer. Passing a duration rather than an absolute deadline avoids relying on clocks agreeing across silos.
- The coordinator must not start the append after its own timeout has passed.
- Possible extra buffer for the commit notification to arrive before the timer fires (not decided). The timer checks the stream anyway, so this buffer would only save a stream read, not affect correctness.

### Commit notification

- Grains have an implicit subscription to a non-replayable stream that announces successful transactions. Delivery is expected in under a second.
- It covers the case where `CommitAsync` was never delivered: the grain refreshes from its stream when it hears the transaction committed.

## Existing building blocks

- Event metadata already records the transaction ID, the total event count, the event count for the event's own stream, and the event's index. Single-stream appends currently set both counts to the same value, so the format can already describe an append spanning streams.
- `ILogConsistentStorage.GetTransactionIdAsync` reads back the transaction ID at a version, which lets a writer find out whether an append that threw was actually committed.

## Open questions

- Is `PrepareAsync`/`CommitAsync`/`RollbackAsync` the right model? It was considered, not confirmed.
- How does a secondary grain's in-memory state reach the committed version after `CommitAsync`: apply the prepared events, or refresh from the stream?
- Are secondary grains always on the same KurrentDB connection as the coordinator? A transaction cannot span connections.
- How do subscribers and projections handle a transaction spanning several streams without serving a half-applied state, given the rule that events with an active cursor are never dropped?
- Multi-tenancy: can a transaction span tenants, and if not, where is that enforced?
- What publishes the commit notification, and on which stream provider?
- Which KurrentDB client version adds `appendRecords`, and is it AOT compatible?
