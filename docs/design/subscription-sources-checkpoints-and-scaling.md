# Subscription Sources, Cursors, Checkpoints, and Scaling

Status: **Agreed design, not yet implemented.** Companion to [multitenancy-and-partitioning.md](multitenancy-and-partitioning.md).

## 1. Core principle

A read position (KurrentDB `$all` commit position, stream revision, Cosmos DB LSN) is only meaningful **within one database**. Every cursor and checkpoint therefore names the database it came from explicitly — never hidden inside another field (e.g., the partition).

## 2. Naming

| Name | Meaning | Part of the key? |
|---|---|---|
| `SourceConnectionName` | The logical connection events are **read from**. Shared or dedicated tenant database. | **Yes** |
| `CheckpointConnectionName` | Where checkpoints are **stored**. Many sources may share one checkpoint database. | No |

- Options `ConnectionName` is renamed to `SourceConnectionName`.
- `SourceConnectionName` is **required**; no default.
- `SourceConnectionName` and subscription names are **permanent logical identifiers**. Connection strings may change (rebrand, migration, rotation); names never do. Choose neutral names (`tenant-0042`, `shared-eu-1`), not brand names. Renaming = new subscription starting from the beginning.
- Optional later feature: startup checkpoint migration (e.g., `PreviousNames`) to copy checkpoints to a new key before subscriptions start. Expected to be rare.

## 3. Streaming cursor

```
SourceConnectionName → Topic → Partition → SequenceNumber → SubSequenceNumber
```

- `Topic`: `$all`, a stream/category, or a tenant index stream (`tenant-{TenantId}`).
- `Partition`: real partition meaning only (stream key, hash partition, or `"0"` when unpartitioned).
- `SequenceNumber` meaning is defined by the topic (`$all` commit position vs. stream revision).
- Cursors are only ordered relative to each other when `SourceConnectionName` and `Topic` match; comparing across sources is undefined and must not silently compare numbers.

## 4. Checkpoints

```
SourceConnectionName → SubscriptionName → Partition → position
```

- Replace loose `(subscriptionName, partitionId)` parameters on `ICheckpointStore<T>` with a `CheckpointKey(SourceConnectionName, SubscriptionName, Partition)` record. The topic is part of the subscription's definition, so each checkpoint stays bound to one topic.
- Physical mapping (separators must be chosen so names containing `-` cannot collide):
  - KurrentDB: checkpoint stream per key, e.g. `checkpoint-{source}-{subscription}-{partition}`.
  - Azure Table: `PartitionKey = {source}|{subscription}`, `RowKey = {partition}`.
  - Grain checkpoint store: grain key contains all three parts.
- **Checkpoints are per source connection, not per tenant.** Tenants on a shared connection share one checkpoint unless a tenant topic/partition is used (section 8).
- No legacy fallback for previously stored checkpoints (nothing has shipped).

## 5. Source connection catalog (decision 1)

- Default source: `IConfiguration` (Key Vault via the configuration provider with `ReloadInterval`); custom catalogs supported (tenant DB, tenant service).
- Abstraction: `ISourceConnectionCatalog<TSettings>` exposing the current `{SourceConnectionName → settings}` map plus a change token.
- Layout (section keys are the names; one catalog per provider type):

```json
"Continuum": {
  "SourceConnections": {
	"KurrentDB": { "shared-eu-1": { "ConnectionString": "..." } },
	"CosmosDB":  { "tenant-0042": { "Endpoint": "...", "DatabaseName": "..." } }
  }
}
```

- Per-source settings reserved for scaling: topic mode, partition count (defaults: `$all`, 1).
- Tenant catalog (tenant → `SourceConnectionName`) is separate; subscriptions only consume the source catalog.
- Onboarding order: create database → store secret → add catalog entry (the entry is the trigger).
- Robustness: skip/log invalid or partially reloaded entries; debounce change notifications; removal stops instances and keeps checkpoints.
- Bind with the configuration binding source generator (AOT-safe).
- Clients come from a lazy, cached client provider (`IKurrentDBClientProvider` / Cosmos equivalent) built from the catalog, replacing keyed singletons fixed at startup; disposes on removal, rebuilds on secret change.

## 6. Hosting: grain per subscription unit (decisions 2 and 3)

- **Subscription unit / grain key:** `{Subscription}/{Source}/{Topic}/{Partition}` — included from day one even when always `$all`/`0`.
- A per-silo coordinator (`IHostedService`) watches the catalog and calls idempotent `EnsureRunning()` / `StopAsync()` on unit grains.
- Orleans single-activation gives single ownership across workers (replaces leader leases); placement/rebalancing spreads units across silos.
- Grains run the read loop in the background (non-blocking turns), keep themselves alive (timer/`DelayDeactivation`), and recover after silo loss via reminder or periodic coordinator reconcile, resuming from checkpoints.
- Handlers must be idempotent (catch-up resume and rare duplicate activations).
- Status/metrics are reported per unit.
- **No restart needed** to add a source connection for self-hosted subscriptions.
- The Orleans KurrentDB **stream provider** fixes its queues at silo start; adding sources there requires a restart/rolling deploy.
- Grain hosting is Orleans-dependent (exempt from running under Native AOT); libraries must still have no AOT/trim warnings.
- Non-Orleans workers may keep a simple per-connection hosted path.

## 7. Why not one reader per tenant on `$all`

On a shared database, tenant is not a server-filterable prefix (`{GrainType}-{TenantId}|{Key}`), so N tenant readers would each scan the full log. Cosmos change feed is partitioned by `/streamName`, with the same issue. Reading is cheap; handler processing is the expensive part — scale processing, not reading.

## 8. Scale path

| Stage | Topic | Partitions | Use |
|---|---|---|---|
| Start | `$all` | 1 | Everything |
| Scale A | `$all` | N (stream-key hash) | Hot sources, any topology |
| Scale B | `tenant-{TenantId}` index | 1..N | Shared databases where per-tenant reading justifies the cost |
| Scale C | `$all` relayed to Event Hubs/Kafka | Broker partitions | Many consumers, elastic processing, non-Orleans consumers |
| Later (opt-in) | any | any | Tenant-affinity grain placement |

- **Scale A:** one reader per (subscription, source) dispatches to partition grains by hash of stream key (preserves per-stream order). Restart from the minimum partition checkpoint; partitions ahead skip already processed positions. Bounded queues for back-pressure. Partition count is permanent once set (like names).
- **Scale B:** a projection links events into `tenant-{TenantId}` (like `$by_category`). Costs: extra link writes and projection load on the leader, slight lag, position is stream revision. Only for shared databases (dedicated databases already use `$all`). Cosmos equivalent (tenant-prefixed hierarchical partition key + feed range) must be verified first.
- **Scale C (broker relay):** one relay unit per (relay subscription, source) reads `$all` once and publishes to Event Hubs/Kafka, partitioned by stream key (or tenant ID) to preserve per-stream order. The relay checkpoints with the normal `CheckpointKey`. The broker handles partition balancing, consumer scale-out, retention, and replay; many consumer groups add no source database load; multiple sources can merge into one topic; generalizes the planned cross-tenant integration-event path. Broker consumers use the same cursor shape (`Source` = namespace/cluster, `Topic` = hub/topic, `Partition` = broker partition, `SequenceNumber` = offset). Costs: extra infrastructure, at-least-once delivery (idempotent consumers), two-hop latency, per-partition ordering only, broker partition counts are effectively permanent, message size limits (use claim-check for large payloads), and the Orleans Event Hubs stream provider fixes queues at silo start (adding a source to the relay needs no restart). Prefer Scale A when avoiding extra infrastructure.
- **Tenant-affinity placement:**

## 9. Implementation order

1. Revert the previous partition-as-connection attempt (keep the category test ordering fix).
2. Cursor contract (`IStreamedEventCursor`) and implementations with `SourceConnectionName`.
3. Checkpoint contract (`CheckpointKey`, `ICheckpointStore<T>`) and stores.
4. Update callers (KurrentDB checkpoint monitor, KurrentDB and Cosmos self-hosted subscriptions, Orleans stream provider receiver/checkpointer) and rename options to `SourceConnectionName`. No hosting changes.
5. Source connection catalog and client providers.
6. Grain-based subscription units and coordinator.
7. Tests and documentation; optional checkpoint migration later.

## 10. Runtime tenant onboarding for grain storage (decision 4)

**Orleans:** provider *names* are fixed at startup; what sits behind a provider is not. Per-tenant storage is created lazily on first activation, so adding tenants at runtime is supported.

**Orleans.Multitenant (5.0.1):**
- Provides tenant identity from grain keys (`{TenantId}|{Key}`) and stream namespaces, tenant isolation via call filters, and lazily created per-tenant storage via `AddMultitenantGrainStorage<…>(name, addStorageProvider, configureTenantOptions: Action<TOptions, string tenantId>, …)`.
- A new tenant works without a restart if `configureTenantOptions` can resolve the tenant at runtime (dynamic tenant catalog) and the underlying provider can reach the tenant's database with those options.
- Does **not** manage connections/clients, refresh cached per-tenant providers after a secret rotation or tenant move (to be verified against its caching behavior), or anything for subscriptions/cursors/checkpoints.

**Continuum today:**

| Scenario | Runtime? | Why |
|---|---|---|
| New tenant on an existing connection (shared or dedicated) | Yes, if the tenant callback resolves dynamically | `TenantGrainStorage` builds tenant storage lazily via `KurrentDBGrainStorageFactory.CreateMultitenant` (same for Cosmos and log-consistent storage). A hardcoded callback falls back to default storage. |
| New tenant on a new connection | **No** | `CreateTenantOptions` resolves keyed `KurrentDBClient`/Cosmos clients registered at startup. |
| Connection string/secret change | **No** | Tenant storage instances and client singletons are cached. |

**Decision:**
1. Storage, log-consistent storage, and checkpoint stores resolve clients through the shared client provider (`IKurrentDBClientProvider` / Cosmos equivalent) backed by the source connection catalog (section 5), not keyed startup singletons.
2. Tenant → `SourceConnectionName` mapping comes from a dynamic tenant catalog; `configureTenantOptions` only looks it up.
3. Invalidate cached tenant storage when a tenant's connection changes or its client is rebuilt; already-active grains keep their storage until deactivation (acceptable for rotations/moves).
4. Result: adding a tenant requires no restart, except for the Orleans KurrentDB stream provider.
5. Scope: runtime support is guaranteed for Continuum's connection-based providers (KurrentDB, Cosmos DB storage, event sourcing, checkpoints). Third-party providers used with Orleans.Multitenant support it only if their options can be built from catalog data at runtime.
6. Implemented in step 5 (source connection catalog and client providers).

## 11. Open decisions

- None currently.
