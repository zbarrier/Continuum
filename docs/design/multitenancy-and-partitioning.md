# Design: Multi-Tenancy, Partition-Aware Checkpoints, and Projection De-duplication

Status: **Partially implemented.**
- Partition-aware checkpoints and per-partition projection watermarks (sections 1-2): implemented, uncommitted on branch `initial-cleanup/add-streaming-projects`.
- Provider-supplied de-duplication (section 3): agreed design, not implemented.
- Multi-tenancy (section 4): agreed design, not implemented. It is the final step before the 1.0.0 release.

This document records decisions and the reasoning behind them, so later work does not reopen settled questions.

---

## 1. Partition-aware checkpoints (implemented)

### Decisions
- `ICheckpointStore<TPosition>` methods take a `partitionId`:
  - `GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken)`
  - `StoreCheckpointAsync(string subscriptionName, string partitionId, TPosition position, CancellationToken)`
- Partition IDs come from the provider. Providers that support multiple partitions discover them during initialization and bind each receiver or checkpointer to a partition. Events also carry their partition on `IStreamedEvent.PartitionId`.
- The grain-backed store (`SubscriptionCheckpointStore<TPosition>`) uses the grain key `$"{subscriptionName}/{partitionId}"`. The connection name is only a DI service key; it is not part of the grain key.
- `SubscriptionCheckpointGrain<TPosition>`:
  - is reentrant and generic; `TPosition : IComparable<TPosition>`, and `BigInteger` is supported by Orleans' built-in codec;
  - updates its checkpoint in memory, writes it to storage every 20 seconds if it changed, and writes it on deactivation;
  - serializes writes with a `SemaphoreSlim`.
- Cancellation tokens are passed directly to grain methods. Do not use `WaitAsync(token)`, which only cancels the caller's wait.

### Partition ID per provider
| Provider | PartitionId |
|---|---|
| KurrentDB / EventStoreDB | `"0"` (single partition, `StreamEventExtensions.DefaultPartitionId`) |
| CosmosDB | change feed `LeaseToken` |
| EventHub / Kafka | partition ID |
| Kinesis | `ShardId` |

### Storage layout
- Azure Table: PartitionKey = subscription name, RowKey = partition ID.
- KurrentDB: checkpoint stream `StreamName.ForCheckpoint($"{subscriptionName}-{partitionId}")`. Names now end in `-0` for KurrentDB. No backward compatibility is needed, since Continuum has not shipped.

### Known side effect
Splits and merges leave behind checkpoints for partitions that no longer exist. They do no harm. A cleanup step can be added later if they pile up.

---

## 2. Projection watermarks per partition (implemented)

`ProjectionState` tracks the last-applied position as **topic → partition → stream key → `StreamSequence`**. The property is `LastSequenceByTopicThenPartitionThenStreamKey`.

### Why partition is part of the key
- Sequence numbers can only be compared within one partition.
- After a repartition (a Kinesis split or merge, or a CosmosDB physical partition split), a stream key can move to a partition with **lower** sequence numbers. A watermark shared across partitions would wrongly drop valid events.
- With a watermark per partition, events on the new partition are applied, and duplicates or older events within the same partition are still ignored.
- `StreamSequence` compares `(sequenceNumber, subSequenceNumber)`.

Tests: `tests/Streaming.Orleans.Tests/ProjectionStateTests.cs`, including `Tracks_Partitions_Independently_Within_A_Stream` and `Does_Not_Reapply_An_Event_Within_The_Same_Partition`.

---

## 3. Provider-supplied de-duplication (agreed, not implemented)

### Problem
One default scheme is not ideal for every provider:
- **KurrentDB** has a single global commit position, so tracking per stream key is unnecessary.
- **CosmosDB** has a small window for duplicates when a partition splits (see 5.2).

### Rejected: putting the provider source on `StreamedEvent`
`ProjectionState` would have to branch on every provider. That couples the abstraction to the implementations, and the saved state would have a different shape per provider.

### Agreed direction
Each provider fills in two fields that `ProjectionState` compares generically:
- a **scope**: what the last-applied position is tracked per;
- a **position** within that scope, compared as a tuple.

`ProjectionState` keeps one map from scope to last-applied position, with no provider-specific code.

| Provider | Scope | Position |
|---|---|---|
| KurrentDB | connection name (constant per database) | (commit position, prepare position) |
| CosmosDB | stream key | stream version (`ver`), or (`_lsn`, `subSeq`) |
| EventHub / Kafka / Kinesis | partition + stream key | (sequence, subsequence) |

### Notes
- **KurrentDB:**
  - Events appended in one write can share a commit position, so the position must include the prepare position. Comparing only the commit position would drop the second and later events of a multi-event write.
  - A projection must never get events from more than one KurrentDB database. Using the connection name as the scope still protects against that, and against a database per tenant.
  - The result is one position per projection, so state no longer grows with the number of streams. This is correct because `$all` is read in order through a single partition.
- **CosmosDB:**
  - `_lsn` works like a transaction ID: all events written in one transaction share it, and `subSeq` (set by Continuum's writer) orders them.
  - A transactional batch is limited to one partition key, so a transaction never spans leases.
  - Using `ver` as the position also closes the duplicate-at-split gap.
- **Saved state:** the shape of a projection's saved state must stay stable. Changing a projection's strategy means rebuilding the projection.

---

## 4. Multi-tenancy (agreed, not implemented)

### Library
Use **Orleans.Multitenant** (https://github.com/VincentH-Net/Orleans.Multitenant):
- **Requirement:** Orleans **10.4+**. Continuum is on 10.3.1, so it must be upgraded first.
- **Grain keys:** the tenant goes in the grain key as `{TenantId}|{Key}`. The separator `|` is hard-coded and cannot be configured. A grain with no tenant uses the plain key.
- **Grain IDs:** the grain type is **not** in the key; Orleans keeps it separately in `GrainId.Type`.
- **Helpers to use:**
  - `ForTenant(tenantId)` on `IGrainFactory`
  - `GetTenantId()` and `GetKeyWithinTenant()` on a grain, `GrainId` or `StreamId`
  - `GetTenantStreamProvider(...)`
- **What it enforces:**
  - grain call and stream filters throw `UnauthorizedAccessException` on access across tenants;
  - `ICrossTenantAuthorizer` allows specific cross-tenant access;
  - per-tenant storage provider instances, configured with `configureTenantOptions(options, tenantId)`.
- **Grain keys:** only `IGrainWithStringKey` grains can be tenant-specific. Continuum's event sourcing already requires string keys.
- **AOT caveat:** its internals read Orleans' internal `Grain.ServiceProvider` and `Grain.GrainFactory` properties through reflection, which is not AOT/trim-safe.
  - Continuum libraries must be `IsAotCompatible` with no warnings.
  - So only application or silo code should reference the package.
  - If Continuum libraries need key handling, fork just the small key-format helpers (about 100 lines) into Continuum.

### Stream name format
```
{GrainType}-{TenantId}|{Key}     tenant           e.g. Order-acme|3f2a9c1e-...
{GrainType}-{Key}                no tenant        e.g. Order-3f2a9c1e-...
```
In other words, the stream name is the grain type, then `-`, then the Orleans grain key exactly as the library builds it. Converting between a `GrainId` or `StreamId` and a stream name is just concatenating, or splitting on the **first** `-`.

### Character rules (validate when IDs are created)
| Part | `-` allowed | `\|` allowed | Other |
|---|---|---|---|
| GrainType | No | No | KurrentDB's category ends at the first `-` |
| TenantId | Yes | No | Must be non-empty; the library treats an empty tenant as "no tenant" |
| Key | Yes (GUIDs) | No | Disallow a leading `~`, or always use the library's helpers (see below) |

- A `|` after the category means the name has a tenant; no `|` means it doesn't. One parser handles both cases, and grains need no flag.
- The library escapes a `|` inside a tenant ID as `||`. Disallowing `|` means that escaping never happens.
- The library inserts a `~` after the separator when the key within a tenant starts with `|` or `~`. Always build and split keys with the library's helpers rather than by hand.
- `|` is allowed in KurrentDB stream names and CosmosDB `id` values. Avoid `/ \ ? #`, which CosmosDB `id` values don't allow, and `$`, which marks KurrentDB system streams.

### Why the tenant goes after the category
- KurrentDB splits the category on the first `-`, so `$ce-Order` covers **all tenants** with no setup per tenant.
- Subscriptions stay unaware of tenants. Tenant isolation is enforced by the library's filters, not by how subscriptions are set up.
- Many tenants can share a KurrentDB database without per-tenant configuration.
- One tenant's events of one type can still be read by filtering `$all` on the prefix `Order-acme|`.
- Rejected: tenant first (`acme|Order-...`). It gives a category per tenant (`$ce-acme|Order`), which would force subscriptions to know about tenants.

### Read path (subscription → Orleans stream)
- Split the stream name on the first `-`. The remainder is the Orleans stream key, used **as-is** so it matches the library's `{tenantId}|{key}` format.
- The library's stream filters then route and authorize by tenant.

### Storage per tenant and sharing clients
- Per-tenant provider instances set `ConnectionName` in `configureTenantOptions` from a tenant-to-connection map (default `"shared"`).
- KurrentDB providers resolve `GetRequiredKeyedService<KurrentDBClient>(ConnectionName)`. Tenants on the same database therefore share **one** client, so there is one client per database, not per tenant.
- A tenant that needs its own database gets its own connection name and client.
- Things to fix or verify:
  - `KurrentDBStateManager` (`src/Streaming.Orleans.KurrentDBStreamProvider/Providers/Storage/KurrentDBStateManager.cs`) falls back to `new KurrentDBClient(_options.ClientSettings)` when no connection name is set. That would create a client per tenant. Require the connection name, or make sure tenant options always set it.
  - Shared clients must be owned by DI. A tenant provider must never dispose a shared client.
- With a database per tenant, checkpoints and the KurrentDB de-duplication scope must include the tenant or connection name, for example a subscription per tenant database.

### Architecture rule
- Events read straight from KurrentDB are pure **domain events** from one database. Never mix events from multiple KurrentDB databases in one projection.
- Reports that span tenants or sources consume **integration events**. A separate step publishes them from each source to EventHub, Kafka or Kinesis.

### Integration checklist
1. Upgrade Orleans to 10.4+.
2. Add a stream name builder and parser in Continuum following the rules above, with validation. Tests should cover names with and without a tenant, GUID keys, and invalid characters.
3. Event-sourced grains: build stream names as `GrainType + "-" + grainId.Key`.
4. Stream providers and subscriptions: build the Orleans `StreamId` key from the stream name's remainder.
5. Per-tenant storage: register providers with `AddMultitenantGrainStorage` and map tenants to connection names. Pass `TypeMappedJsonGrainStorageSerializer(requireStoredType: true)` to tenant instances of event-sourcing providers.
6. Remove or guard the `KurrentDBStateManager` client fallback.
7. Confirm AOT: Continuum library builds stay free of AOT/trim warnings.

---

## 5. Provider notes and known limitations

### 5.1 Kinesis (not a priority)
- The Orleans Kinesis provider uses the AWS SDK directly, not KCL. KCL needs a Java MultiLangDaemon, and its DynamoDB leases would compete with Orleans' queue balancer.
- Shards are discovered once, in `KinesisAdapterFactory.CreateAdapter`, and fixed in a hash-ring queue mapper.
- `KinesisShardTopologyMonitor` checks the shard list every `TopologyCheckInterval` (default 1 minute), and immediately when a shard is exhausted. If the list changed, **all receivers stop**: "Live resharding is not supported ... Restart the Orleans stream provider."
- Stopping does not lose data: closed parent shards stay readable for the retention period, and checkpoints are kept per shard. Data is lost only if retention expires before a restart.
- After a restart, parent and child shards are read at the same time, so a key's events can arrive **out of order**.
- Fix if needed later (KCL-style, pure .NET):
  1. Mark a shard as finished in its checkpoint when its iterator is exhausted.
  2. Start a child shard only after its `ParentShardId` / `AdjacentParentShardId` checkpoints are marked finished.
  - This fits the per-partition checkpoint store. Resharding without a restart would still require a queue mapper that can add queues at runtime.

### 5.2 CosmosDB change feed
- `partitionId = context.LeaseToken`, `sequenceNumber = _lsn`, `subSequenceNumber = subSeq`. This mapping is correct.
- Moving a lease between hosts does not change its lease token. Partition splits and merges do, and the child leases start from the parent's continuation token.
- The change feed processor writes the lease checkpoint after the delegate completes successfully, and discovers a split on its next read (partition gone). So a handled batch is normally checkpointed before the split takes effect.
- Remaining gap: the checkpoint write fails or the host crashes, then the partition splits before a retry. The batch is then replayed under the child lease tokens and isn't recognized as a duplicate. This is acceptable for at-least-once delivery; using `ver` as the position (section 3) closes it.
- References: https://learn.microsoft.com/azure/cosmos-db/nosql/change-feed-processor and the azure-cosmos-dotnet-v3 `ChangeFeedProcessor` source.

### 5.3 EventHub
- `EventHubAdapterFactory.CreateAdapter` discovers partition IDs, builds the queue mapper, maps each `QueueId` to a partition, and creates one checkpointer per partition.
- Sequence tokens are only ordered within a partition, which is why watermarks are tracked per partition.
