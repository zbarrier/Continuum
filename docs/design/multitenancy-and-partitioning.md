# Design: Multi-Tenancy, Partition-Aware Checkpoints, and Projection De-duplication

Status: **Implemented.**
- Partition-aware checkpoints, projection watermarks, and provider cursor scopes (sections 1-3): implemented.
- Multi-tenancy (section 4): implemented for KurrentDB event sourcing, CosmosDB event sourcing and KurrentDB grain storage. A database per tenant for subscriptions is not implemented (see "Storage per tenant and sharing clients").

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

## 2. Projection watermarks (implemented)

`ProjectionState` skips events it has already applied using `ProjectionWatermarks`, which stores one `(sequenceNumber, subSequenceNumber)` watermark per **topic + partition**. Each event's `IStreamedEventCursor` supplies the topic, partition, and position, so `ProjectionState` has no provider-specific code.

### Why partition is part of the key
- Sequence numbers can only be compared within one partition.
- After a repartition (a Kinesis split or merge, or a CosmosDB physical partition split), a stream key can move to a partition with **lower** sequence numbers. A watermark shared across partitions would wrongly drop valid events.
- With a watermark per partition, events on the new partition are applied, and duplicates or older events within the same partition are still ignored.

Tests: `tests/Streaming.Orleans.Tests/ProjectionStateTests.cs`.

---

## 3. Provider cursor scopes (implemented)

Each provider chooses the topic and partition so that events reach a projection in position order within that scope.

| Provider | Topic | Partition | Position |
|---|---|---|---|
| KurrentDB self-hosted subscription (`$all`) | `$all` | `"0"` | (commit position, 0) |
| KurrentDB Orleans stream provider | stream category | stream key (`{TenantId}|{Key}` when tenant-qualified) | (commit position, 0) |
| CosmosDB change feed | monitored container | `LeaseToken` | (`_lsn`, `subSeq`) |
| EventHub / Kafka / Kinesis | topic | partition / shard | (sequence, subsequence) |

### Notes
- **KurrentDB self-hosted:**
  - `$all` is read in commit-position order and every event has its own commit position, so one watermark per projection is enough and state does not grow with the number of streams. The prepare position is not needed.
  - Each grain must receive its events in order: await each call or send the grain's events as one ordered list; never call the same grain in parallel.
  - **Only one self-hosted subscription may call a given grain.** A grain fed by two subscriptions would see one subscription's positions as already applied and skip the other's events. This is also a code smell: the subscription reads `$all`, so the one feeding a grain should subscribe to every event that grain needs.
  - A projection must never get events from more than one KurrentDB database.
- **KurrentDB Orleans stream provider:** Orleans delivers each stream independently and does not order one stream against another, so the scope stays per stream.
- **CosmosDB:**
  - `_lsn` works like a transaction ID: all events written in one transaction share it, and `subSeq` (set by Continuum's writer) orders them.
  - A transactional batch is limited to one partition key, so a transaction never spans leases.
- **Saved state:** changing a projection's cursor scope means rebuilding the projection.

---

## 4. Multi-tenancy (implemented)

### Library
Applications use **Orleans.Multitenant** (https://github.com/VincentH-Net/Orleans.Multitenant). Continuum libraries do not reference it:
- **Requirement:** Orleans **10.4+**. Continuum is on 10.4.0.
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
  - So only application or silo code references the package.
  - Continuum reproduces the key format in `Continuum.Orleans.Tenancy.TenantKey` (`Create` and `Split`), which is AOT-safe and produces the same keys as the library.

### Stream name format
```
{GrainType}-{TenantId}|{Key}     tenant           e.g. Order-acme|3f2a9c1e-...
{GrainType}-{Key}                no tenant        e.g. Order-3f2a9c1e-...
```
In other words, the stream name is the grain type, then `-`, then the Orleans grain key exactly as the library builds it. Converting between a `GrainId` or `StreamId` and a stream name is just concatenating, or splitting on the **first** `-`.

This format is KurrentDB-specific and lives in `Continuum.Orleans.KurrentDB.KurrentDBStreamName` (`Format` and `ValidateCategory`). The store-neutral `StreamNameParsing` only splits a category from its remainder and reads the tenant from the remainder; each store decides its own separator.

| Store | Event stream name | Grain state stream name |
|---|---|---|
| KurrentDB | `{GrainType}-{TenantId}\|{Key}` | `state__{GrainType}-{TenantId}\|{Key}` |
| CosmosDB | `{ServiceId}/{GrainId}` (the tenant is inside the grain key, parsed at the last `/`) | n/a |

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
- Orleans.Multitenant's `AddMultitenantGrainStorage` builds tenant options from scratch, without the provider's settings or the connection-based serializer and type mapper, so it cannot be used with Continuum providers. Continuum has its own registrations instead:
  - `AddMultitenantKurrentDBBasedLogConsistencyProvider` (KurrentDB event sourcing)
  - `AddMultitenantAzureCosmosDBBasedLogConsistencyProvider` (CosmosDB event sourcing)
  - `AddMultitenantKurrentDBGrainStorage` (KurrentDB grain state)
- Each takes `configureTenantOptions(tenantId, options)` plus the normal options callback. A tenant's options start from a copy of the provider's options; grains without a tenant use the provider's own storage. When the callback changes `ConnectionName`, the serializer and type mapper are resolved again for the new connection unless the callback set them.
- The routing is done by `TenantLogConsistentStorage` and `TenantGrainStorage`. A tenant's storage is created and validated on first use and reused afterwards. If creation fails, the error is rethrown to the caller and the next call retries.
- `TenantStorageFailureLogger` logs every creation failure as an error, with the tenant ID, provider name and failure count. After a tenant has been failing for 5 minutes, it logs at most once a minute for that tenant, with the number of failures not logged. A later success is logged once and resets the tenant.
- Per-tenant provider instances set `ConnectionName` in `configureTenantOptions` from a tenant-to-connection map (default `"shared"`).
- KurrentDB providers resolve `GetRequiredKeyedService<KurrentDBClient>(ConnectionName)`. Tenants on the same database therefore share **one** client, so there is one client per database, not per tenant.
- A tenant that needs its own database gets its own connection name and client.
- `ConnectionName` is required by every KurrentDB provider, including `KurrentDBStateManager`; the stream provider's `ClientSettings` fallback was removed.
- Shared clients are owned by DI. Storage providers, including tenant instances, never dispose a client.
- With a database per tenant, checkpoints and the KurrentDB de-duplication scope must include the tenant or connection name, for example a subscription per tenant database.

### Architecture rule
- Events read straight from KurrentDB are pure **domain events** from one database. Never mix events from multiple KurrentDB databases in one projection.
- Reports that span tenants or sources consume **integration events**. A separate step publishes them from each source to EventHub, Kafka or Kinesis.

### Integration checklist
1. ~~Upgrade Orleans to 10.4+.~~ Done.
2. ~~Add a stream name builder and parser in Continuum following the rules above, with validation.~~ Done: `TenantKey`, `StreamNameParsing`, `KurrentDBStreamName`.
3. ~~Event-sourced grains: build stream names as `GrainType + "-" + grainId.Key`.~~ Done.
4. ~~Stream providers and subscriptions: build the Orleans `StreamId` key from the stream name's remainder.~~ Done.
5. ~~Per-tenant storage, mapping tenants to connection names.~~ Done with Continuum's own multitenant registrations (see above). Tenant instances of event-sourcing providers keep `TypeMappedJsonGrainStorageSerializer(requireStoredType: true)` because the serializer is resolved per connection.
6. ~~Remove the `KurrentDBStateManager` client fallback.~~ Done.
7. ~~Confirm AOT: Continuum library builds stay free of AOT/trim warnings.~~ Done.
8. Database per tenant for subscriptions (checkpoints and de-duplication scoped by tenant or connection): not done.

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
