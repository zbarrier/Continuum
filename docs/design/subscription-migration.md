# Subscription Migration (Design Notes)

Status: **Proposed, not implemented.** Captured from a design discussion so nothing is lost when the feature is built.

Related: [subscription-sources-checkpoints-and-scaling.md](subscription-sources-checkpoints-and-scaling.md) is a previous
design doc that overlaps with this one (hosting, checkpoint keys, naming, partitioning). It was written in a different
context; review both when implementing.

## Goal

Rebuild a projection efficiently, then hand it off to live Orleans projection grains:

1. A **self-hosted KurrentDB catch-up subscription** reads `$all` from the beginning and batch-writes the projection to
   SQL Server (fast bulk load).
2. When it catches up, it **hands off** to the **KurrentDB Orleans stream provider** catch-up receiver, which continues
   from the exact position with projection grains.
3. The new projection grains (e.g. `OrderProjection.V2`) are **not used by the application** until the handoff is done
   and the grains have caught up. Then the UI is switched from V1 to V2.

## Chosen design: handoff through a KurrentDB migration stream

### Self-hosted subscription (stays a hosted service for now)

- When it has caught up, it stops processing and appends a `CaughtUp` event to
  `SubscriptionMigration-{SubscriptionName}` carrying the `$all` **commit position P** of the last event it processed
  and committed.
  - P must be the last *processed and checkpointed* position, not merely where the `CaughtUp` signal arrived. Events
	after the decision to hand off must not be processed; they belong to the receiver.
  - Confirm whether the KurrentDB client's `CaughtUp` message carries a position; the safe choice is the subscription's
	own last processed position.
- Append with `StreamState.NoStream` and a deterministic event id so it is written exactly once (safe on retry after a
  crash, and against a second instance).
- It then stops itself, so there is no overlap. On later starts it sees the migration event already exists and does
  nothing, so no config flag is needed to disable it.
- Add a new health/status state **`Migrated`**:
  - Reported as **Healthy** (finished on purpose), with no exception attached (the health checks treat any status with
	an exception as failing).
  - Include the handoff position P in the health data for traceability.
  - Lets operators see which subscriptions are active and which have migrated.
- A grain-hosted version was considered (single activation across the cluster, deactivates itself when done). Deferred;
  self-hosted is fine for now. If revisited: needs `DelayDeactivation`/timer to stay alive, a reminder to restart after
  a crash or rebalance, and care with grain scheduling for the background read loop.

### Orleans provider catch-up receiver (silo side)

New start mode alongside `StartFromNow` / from-beginning: **wait for migration**.

Startup order:

1. If the receiver already has its own checkpoint, use it and ignore the migration stream (already migrated).
2. Else read `SubscriptionMigration-{SubscriptionName}`. If the `CaughtUp` event exists, start `$all` from
   `FromAll.After(P)`.
3. Else subscribe to the migration stream and wait for the event, then switch to `$all` from `After(P)`.

Notes:

- **Do not wait in `InitAsync`.** Orleans limits receiver init time. Do the wait in the background pump
  (`PumpAsync` in `KurrentDBCatchUpSubscriptionReceiver`), which runs in two stages: migration stream, then `$all`. The
  receiver returns no messages while waiting.
- `After(P)` gives no gap and no overlap; events written during the handoff are picked up regardless of when the
  receiver starts.
- The migration event itself appears in `$all` after P. Filter the `SubscriptionMigration-` prefix out, or make sure no
  projection reacts to it.
- Report the receiver's `CaughtUp` from `$all` (status/health), so operators know V2 is ready.
- Rebuilds: don't tombstone (hard-delete) the migration stream. Prefer a versioned name such as
  `SubscriptionMigration-{Name}-v{n}`, so a rebuild is a version bump. A rebuild must also reset the receiver checkpoint.

### Shared contract

- Migration event type and stream-name convention live in the Abstractions project, used by both sides.
- Positions only line up on `$all`; a single-stream self-hosted subscription can't hand off to the provider's `$all`
  checkpoint.

### Projection grain state

- V2 projection grains use a **SQL Server grain storage provider** (an existing Orleans provider or a custom one), so
  the grains read what the bulk load wrote.
- The bulk load and the grain logic must compute identical projections. Shared, pure projection code (`When`/`Apply`)
  prevents divergence at P.
- Grain watermarks (`ProjectionState.Watermarks`) skip events at or before P if there is any overlap.

## Current code gaps (as of this discussion)

| Need | Today |
|---|---|
| React to `CaughtUp` in the self-hosted subscription | Only logged in `KurrentDBCatchupSubscription.SubscribeToAll`; no hook. |
| Self-hosted checkpoint | Owned by the base class via `KurrentDBCheckpointMonitor`, keyed by subscription name and partition `"0"`; separate from SQL writes, so not atomic with them. Its key does not start with the connection name, unlike the repo's cursor convention (SourceConnectionName first); worth fixing. |
| Choose where the provider starts | `KurrentDBCatchUpSubscriptionReceiver.GetStartPositionAsync`: checkpoint, else `StartFromNow` (default true), else from beginning. No migration mode or seeded position. |
| Provider checkpoint key | `"{ConnectionName}-{ConsumerGroup}"`, partition `"0"`, `$all` commit position. |
| Grains skip already-applied events | Supported via `ProjectionState.Watermarks`. |

### Exactly-once into SQL

For exactly-once bulk loading, let subclasses own checkpoint storage so the SQL transaction includes the checkpoint.
Otherwise use upserts that tolerate duplicates (at-least-once).

### Single instance

The self-hosted subscription runs per process. In a multi-silo or multi-host deployment, make sure only one instance
bulk-loads (this was a reason to consider the grain-hosted version).

## Switching the application from V1 to V2

### Side by side

- `OrderProjection.V2` is a separate grain interface and class (not Orleans interface versioning).
- V2 has its own consumer group (own checkpoint and migration stream) and its own SQL storage.
- V1 keeps running unchanged until it is retired.

### UI switch: page-level feature flag, no shared interface

- A page checks a flag and renders `OrderDetailsV1` or `OrderDetailsV2`. Each component references its own projection
  grain directly.
- A shared `IOrderQueries` interface was rejected: a new projection usually means different data and a different UI,
  so a common view model would lose V2 data or make V1 return empty fields. Only use a shared interface when V1 and V2
  return identical data.
- One flag per migration (e.g. `OrderProjectionV2`), checked once at the page level, used by every page that shows
  orders.
- APIs: add V2 as new routes (`/v2/orders/...`) beside V1; use the flag server-side only if the route must stay the same.
- Put a removal date on the flag; delete the flag, V1 component and V1 grain reference together when V1 is retired.

### Feature flags

- Preferred library: **Microsoft.FeatureManagement** (`IFeatureManager.IsEnabledAsync("OrderProjectionV2")`), in the
  application's front end, not in Continuum. To be discussed further when building the app.
- Start with flags in `appsettings.json`. Azure App Configuration can be added later for portal toggles, targeting and
  no-restart switching across instances; only the setup lines change.
- Blazor Server notes:
  - App Configuration's refresh middleware runs on HTTP requests, not SignalR; use a background service calling
	`IConfigurationRefresher.TryRefreshAsync()` if App Configuration is used.
  - Check the flag in `OnInitializedAsync` so users switch on page load; use `IFeatureManagerSnapshot` for a consistent
	answer per circuit.
  - Targeting needs an `ITargetingContextAccessor` (user identity and groups).
- The silos don't need the flag; both projections run there.

## Rollout order

1. Deploy silos with V2 grains, storage and receiver (wait-for-migration mode). V1 untouched.
2. Self-hosted bulk load runs, appends `CaughtUp(P)` and reports `Migrated`.
3. V2 receiver starts at `After(P)` and catches up.
4. Verify V2 data (e.g. compare samples between V1 and V2 through an admin page).
5. Turn the flag on (optionally admins first). Turn it off to roll back; V1 is still live.
6. After a stable period, retire V1: stop its subscription, remove its grain, registration and storage, and delete the
   flag.

Silos must always be deployed before a front end that references V2 grains.

## Implementation checklist (when the time comes)

1. Self-hosted: `CaughtUp` handling plus a migration mode that appends `CaughtUp(P)` with `NoStream` and stops.
2. Self-hosted: `Migrated` status in the status registry, health check and metrics.
3. Abstractions: migration event type and stream-name convention.
4. Receiver: wait-for-migration start mode with the two-stage pump; filter migration events from `$all`.
5. Receiver: report caught-up status.
6. Optional: subclass-owned checkpoints for transactional SQL bulk loads.
7. Optional: connection-name-first self-hosted checkpoint key.
8. Tests: handoff with no gap or overlap, restart before and after the handoff, duplicate append, receiver already
   checkpointed.
