# Continuum.Streaming.Orleans.KurrentDB.StreamProvider

An Orleans persistent stream provider for [KurrentDB](https://www.kurrent.io/). Orleans runs the subscription and delivers events to grains through Orleans streams. Use it when grains, such as projections, should react to events written to KurrentDB.

There are two ways to read from KurrentDB:

- **`$all` catch-up subscription** (recommended). Events are delivered in order, none are dropped, and the read position is checkpointed by the provider.
- **Single stream catch-up subscription.** The same guarantees, reading one stream such as `$ce-{Category}` or a custom projection stream instead of `$all`.
- **Persistent subscription.**

If you want to run the subscription yourself in a worker service, use `Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions` instead.

## Stream providers and subscriptions

In this README, a *stream provider* is one named Orleans stream provider registered with `AddKurrentDBStreams("Name", ...)`, not KurrentDB itself. Each stream provider runs exactly one subscription, and its name is that subscription's identity: it keys the checkpoint and qualifies the stream namespaces consumers use. One KurrentDB database can, and usually will, serve many stream providers. Providers that share a `ConnectionName` share one `KurrentDBClient`, so adding a subscription does not add a connection.

## Install

```
dotnet add package Continuum.Streaming.Orleans.KurrentDB.StreamProvider
```

## Configuration from appsettings

`Orleans:Streaming:{Name}`. The section name (`Projections-V1` below) is the stream provider name, which is also the subscription name. Add or remove a section to enable or disable that subscription.

```json
{
  "Orleans": {
    "Streaming": {
      "Projections-V1": {
        "ProviderType": "KurrentDB",
        "SubscriptionType": "AllStreamCatchup",
        "ConnectionName": "kurrentdb",
        "Credentials": { "UseDefault": true },
        "StartFromNow": false,
        "ResolveLinkTos": true,
        "BufferCapacityMultiplier": 10,
        "CheckpointConnectionName": "kurrentdb",
        "CheckpointMonitorOptions": { "MaxEventsBeforeCommit": 10000, "MaxTimeBeforeCommit": "00:00:20" }
      }
    }
  }
}
```

`SubscriptionType` is `Persistent` (default), `AllStreamCatchup`, or `StreamCatchup`. `Queues` holds the stream name for `StreamCatchup` and the queue names for `Persistent`. Persistent subscriptions also read `StartFromNow`, `UpdateExistingSubscription`, and `PrefetchCount`. `UnknownEventTypeBehavior` (`Halt` or `Skip`) applies to every strategy. `StartFromNow` defaults to `false` for catch-up subscriptions, so a new subscription reads its full history, and to `true` for persistent subscriptions. KurrentDB client types (`ClientSettings`, `SubscriptionSettings`) and the rest of the data adapter are not bindable and are only configurable in code.

## `$all` catch-up subscriptions

```csharp
siloBuilder.AddKurrentDBStreams("KurrentDB", configurator =>
{
	configurator.ConfigureKurrentDB(ob => ob.Configure(options => options.Queues = [KurrentDBStreams.All]));
	configurator.UseAllStreamCatchupSubscription(ob => ob.Configure(options =>
	{
		options.Filter = StreamFilter.Prefix("snack-");
		options.StartFromNow = false;
	}));
});
```

### How it works

Each stream provider runs one subscription, so its `Queues` must have exactly one entry, `$all` (`KurrentDBStreams.All`). If `Queues` is empty it defaults to `$all`.

Each event is delivered to the Orleans stream named after the KurrentDB stream it was written to. The name is split on the first `-`, so `snack-123` becomes stream namespace `snack` with key `123`. Keys can contain their own dashes, such as GUIDs. A tenant-qualified name such as `snack-tenantA|123` keeps the tenant in the key. To use a different naming convention, set `StreamIdMapper` on the data adapter options.

Set `Filter` to have KurrentDB send only the events you need. It takes any KurrentDB `IEventFilter` and is set in code, not configuration:

- `StreamFilter.Prefix("snack-")` for streams by name prefix.
- `EventTypeFilter.Prefix("Snack")` for events by type prefix.
- `StreamFilter.RegularExpression(...)` and `EventTypeFilter.RegularExpression(...)` when a prefix is not enough. Prefer prefixes: a regular expression is evaluated on the server against every event in `$all`.

#### System events

`$all` also holds system events, whose types start with `$`: link events (`$>`) written by projections such as `$by_category`, stream metadata, projection state, statistics and the checkpoints written by `KurrentDBCheckpointStore` (`$checkpoint`). By default `Filter` is `EventTypeFilter.ExcludeSystemEvents()`, so none of these are delivered.

Setting your own filter replaces the default, because a subscription takes one filter, either by stream or by event type. A stream prefix such as `snack-` or an event type prefix such as `Snack` does not match system events anyway. A stream prefix that matches link streams, such as `$ce-snack`, delivers those links on purpose, resolved to their events. Set `Filter = null` only if you really want every event, including system events.

Filtered `$all` subscriptions require an admin user. A filter applies from the subscription's checkpoint onwards, so changing it does not revisit events already passed. Link events are resolved to the events they point to by default (`ResolveLinkTos`).

`BufferCapacityMultiplier` (default 10) limits how far the subscription reads from `$all` ahead of the cache, in Orleans reads of up to 1,000 events each, so 10,000 events by default.

A resolved link carries the same event as the original, at a later `$all` position, and the catch-up subscription delivers every position it reads. If the filter matches both the original streams and the streams holding links to them (for example, `Filter = null`, which includes the `$ce-` and `$et-` system projection streams), each event is delivered once per match. Filter to either the original streams (for example `order-`) or the link streams (for example `$ce-order`), not both. Persistent subscriptions drop a redelivered event that is still queued, so this applies to catch-up subscriptions only.

### Checkpoints

The provider stores the `$all` commit position in an `ICheckpointStore<ulong>`.

The checkpoint only moves past an event once every active subscriber has read it and it has been purged from the cache, so a restart never skips an event a subscriber has not seen. Delivery is at least once, so after a restart some events may be delivered again.

Writes to the checkpoint store are batched by `CheckpointMonitorOptions`. By default, a checkpoint is written after 10,000 events or 20 seconds, whichever comes first, and once more on shutdown.

`StreamPullingAgentOptions.BatchContainerBatchSize` must stay at 1 (the Orleans default). A larger size lets the pulling agent read past events before delivering them, so the checkpoint could move past events no subscriber received. A larger size fails validation at startup.

A catch-up subscription is read only. Events are written by event sourcing, so writing to the provider throws.

### Transactions

Events appended in one transaction are delivered together, in one batch, to each Orleans stream the transaction wrote to. The checkpoint never moves into the middle of a transaction.

A filter can let through only part of a transaction. This is allowed, because some filters are meant to do it, but it is never silent. `PartialTransactionHandling` on the catch-up subscription options decides what happens:

- `Halt` (default): logs a critical error and stops the receiver without moving the checkpoint past the transaction. Change the filter or the handling and restart.
- `DeliverAndWarn`: delivers the events received and logs a warning.
- `Deliver`: delivers the events received and logs at debug level.

A transaction is only treated as partial once that is confirmed: an event from another transaction arrives, or, after about 5 seconds with nothing arriving, a short read of `$all` with the same filter finds no further event of it. A read that fails or times out confirms nothing, so the subscription keeps waiting and checks again. A dropped connection can take a while to detect, so silence alone never ends a transaction.

A transaction must fit in one cache block. One that does not halts the receiver with a critical log giving the size it needs and the block size; raise the block size and restart.

### Multiple `$all` subscriptions

You can run as many `$all` subscriptions against the same database as you need. Add one stream provider per subscription, each with a distinct name and the default filter, the same filter, or a different filter. Providers that use the same `ConnectionName` share one `KurrentDBClient`, and each subscription runs as a separate call on that client's connection.

```csharp
siloBuilder.AddKurrentDBStreams("Snacks", configurator =>
{
    configurator.ConfigureKurrentDB(ob => ob.Configure(options =>
    {
        options.ConnectionName = "kurrentdb";
        options.Name = "Snacks";
        options.Queues = [KurrentDBStreams.All];
    }));
    configurator.UseAllStreamCatchupSubscription(ob => ob.Configure(options => options.Filter = StreamFilter.Prefix("snack-")));
});

siloBuilder.AddKurrentDBStreams("Reporting", configurator =>
{
    configurator.ConfigureKurrentDB(ob => ob.Configure(options =>
    {
        options.ConnectionName = "kurrentdb";
        options.Name = "Reporting";
        options.Queues = [KurrentDBStreams.All];
    }));
    configurator.UseAllStreamCatchupSubscription();
});
```

Each subscription has its own filter, cache and checkpoint. The checkpoint is keyed by the connection name, the Orleans service id and the stream provider name, so subscriptions never share a position.

A consumer chooses a subscription by its stream provider name, then a stream by namespace and key. The namespace is always qualified with the provider name (see [Stream namespaces](#stream-namespaces)):

```csharp
var stream = this.GetStreamProvider("Snacks").GetStream<IStreamedEvent<object>>(StreamId.Create("Snacks:snack", "123"));
```

When two subscriptions both match a KurrentDB stream, each delivers its events on its own provider and namespace, so subscribers of one never receive the other's deliveries. This also holds for implicit subscriptions.

so use `Filter` to keep each one to the events it needs.

## Single stream catch-up subscriptions

To read one stream instead of `$all`, such as a category stream built by the `$by_category` system projection or a stream your own projection links events into, name it as the only queue:

```csharp
siloBuilder.AddKurrentDBStreams("Snacks", configurator =>
{
    configurator.ConfigureKurrentDB(ob => ob.Configure(options => options.Queues = ["$ce-snack"]));
    configurator.UseStreamCatchupSubscription(ob => ob.Configure(options => options.StartFromNow = false));
});
```

- `Queues` must have exactly one entry, the stream name. `$all` is rejected; use `UseAllStreamCatchupSubscription` for it.
- There is no `Filter`: the stream itself is the filter.
- `$ce-` streams and custom projection streams hold link events, so keep `ResolveLinkTos` on (the default). `$ce-` streams only exist when the `$by_category` projection is running.
- Each event is delivered to the Orleans stream named after the stream it was originally written to, so `$ce-snack` delivers `snack-123` on namespace `snack` with key `123`, the same as an `$all` subscription.
- The checkpoint is the event's revision in the subscribed stream, not its `$all` position or its revision in its original stream.
- A link whose target was deleted is skipped, and the checkpoint still moves past it.
- Events are delivered one at a time, as with `$all`; nothing waits for the rest of a transaction.
- With `StartFromNow`, a stream that does not exist yet is read from its first event, including after a restart before the first checkpoint.

Checkpoints, cache behavior and the rules under [Checkpoints](#checkpoints) otherwise apply as for `$all`.

### Changing what a subscription reads

A subscription's checkpoint is only meaningful for the source it was written for: an `$all` commit position and a stream revision are different numbers. Treat a stream provider name as permanently bound to its source. To switch between `$all` and a stream, switch streams, or replay from the beginning, add a new stream provider with a new name (for example `Projections-V2`) instead of changing the existing one.

## Stream namespaces

Events are delivered on the Orleans stream namespace `{ProviderName}:{Category}`, where the category is the text before the first `-` of the KurrentDB stream name and the key is the rest. The KurrentDB stream `snack-123` read by the provider `Snacks` is delivered as `StreamId.Create("Snacks:snack", "123")`. With persistent subscriptions, events written through the provider are delivered the same way: a producer writes to `StreamId.Create("chat", key)` and subscribers read `StreamId.Create("{ProviderName}:chat", key)`.

Orleans matches implicit subscriptions by namespace only, so the provider name in the namespace keeps providers apart. This lets two versions of a projection read the same database side by side, for example while a new version rebuilds from the beginning:

```csharp
[ImplicitStreamSubscription("ProjectionsV1:snack")]
public class SnackProjectionV1 : ProjectionGrain<SnackProjectionV1, SnackStateV1> { }

[ImplicitStreamSubscription("ProjectionsV2:snack")]
public class SnackProjectionV2 : ProjectionGrain<SnackProjectionV2, SnackStateV2> { }
```

Each provider has its own checkpoint, so `ProjectionsV2` can start from the beginning of `$all` while `ProjectionsV1` carries on from where it is.

`:` is reserved:

- A stream provider name containing `:` fails validation at startup.
- Writing through the provider to a namespace containing `:` throws.
- Event-sourcing storage rejects a stream category containing `:`, and analyzer `CKDB002` warns about a `[GrainType]` containing it.
- An event read from a KurrentDB stream whose category contains `:` is logged as an error and skipped. It is not delivered or retried, and the checkpoint moves past it.

## Persistent subscriptions

```csharp
siloBuilder.AddKurrentDBStreams("KurrentDB", configurator =>
{
	configurator.ConfigureKurrentDB(ob => ob.Configure(options => options.Queues = ["queue-1", "queue-2"]));
	configurator.UsePersistentSubscriptions(ob => ob.Configure(options =>
	{
		options.SubscriptionSettings = new PersistentSubscriptionSettings(consumerStrategyName: SystemConsumerStrategies.Pinned);
	}));
});
```

The `AddKurrentDBStreams(name, Action<KurrentDBOptions>)` shortcut also selects persistent subscriptions.

### How it works

KurrentDB owns the read position for each consumer group, so no checkpoint store is needed. Each queue can run on a different silo, which spreads the load across the cluster.

Events are acknowledged as soon as they are received, not when a grain has handled them. As a result:

- Events still in the cache are lost if a silo stops unexpectedly.
- Order is not guaranteed. Setting `ConsumerStrategyName` to `Pinned` keeps events from the same stream on the same consumer, which preserves order while the set of consumers is stable. Order can still break when queues move between silos, silos start or stop, or events are retried.

Use persistent subscriptions only for consumers that can tolerate dropped and out-of-order events. Consumers that need ordering and no drops should use `$all` catch-up subscriptions.

An existing persistent subscription is not changed on startup. Setting `UpdateExistingSubscription` to `true` applies `SubscriptionSettings`, but resets the server-side start position and discards the consumer group's progress.

## Event size

- **Events must not exceed 1 MB**, data and metadata combined. This is KurrentDB's default maximum append size.
- **Keep metadata under 16 KB.** Metadata is carried with every event and copied on every delivery, so use it for small values such as correlation and causation ids, tenant and user details and tracing headers, and keep the payload in the event data.

Events are cached in 2 MB blocks, which leaves room for a 1 MB event and the extra space the cache needs to store it. A catch-up subscription halts on an event or transaction that does not fit in a block, without moving the checkpoint past it (see [Transactions](#transactions)). A persistent subscription logs an event that does not fit as an error and skips it, since it has already acknowledged it.

## Cache memory after a catch-up

Every queue on a silo shares one pool of 2 MB blocks. A catch-up can grow the pool well beyond what live traffic needs, so the pool gives free blocks back once demand has stayed lower for a while. Peak usage is measured over a 20 minute window. After 2 windows in a row where fewer blocks were needed than are allocated, free blocks are released down to the larger of the number of active queue caches plus 2, or the window's peak usage plus 2. Blocks that hold cached events are never released. Released blocks are reclaimed by the next gen 2 garbage collection, so process memory drops after that rather than immediately.

Change these values with `ConfigureCacheBufferPool`, or set `TrimWindow` to `TimeSpan.Zero` to keep every block for the life of the silo.

## Streams with no subscribers

Events are only delivered to streams that have an active subscriber. The checkpoint is shared by every stream in a queue, so the provider cannot hold it back for streams that nobody is listening to. When no subscriber is reading from the cache, events stay cached for at least `DataMinTimeInCache` (Orleans default: 5 minutes). After that, they can be purged and the checkpoint moves past them. A subscriber that joins later does not receive those events.

With a catch-up subscription, events an active subscriber has not read yet are never purged, no matter how long they have been cached. A persistent subscription has already acknowledged its events, so its cache purges on age alone and does not wait for subscribers.

If every event must be handled, use implicit subscriptions (`[ImplicitStreamSubscription]`) so a grain is activated for each stream that receives an event. To change the minimum time in cache, configure `StreamCacheEvictionOptions` for the provider.

## Idle streams

This applies to catch-up subscriptions, whose cache tracks how far each subscriber has read. A stream whose subscriber has read every cached event does not hold back purging, so idle streams do not make the cache grow, even while catching up on a backlog. When a new event for the stream is added, its cursor resumes from that event, which is kept until the subscriber reads it. A subscriber that has not finished reading, including one whose delivery failed, holds back purging until it catches up; unread events are never dropped.

Orleans releases a stream after `StreamPullingAgentOptions.StreamInactivityPeriod` (Orleans default: 30 minutes) with no new events and no delivery in progress. Its subscriptions are kept, and the next event for the stream is delivered as normal; Orleans only has to look up the stream's subscribers again. A longer period means fewer lookups for streams with regular gaps, at the cost of a little memory per idle stream.

## Health checks and metrics

Add a silo health check with `builder.Services.AddHealthChecks().AddKurrentDBStreamProviderReceivers()`. It covers the catch-up and persistent queue receivers running on the silo. It reports Unhealthy when a receiver failed to start (Orleans retries it on the next read), Degraded while a receiver is starting or reconnecting after a dropped subscription, and Healthy otherwise. A queue that moves to another silo is removed from this silo's health check. A grain that fails to handle an event does not affect receiver health.

The `Continuum.Streaming.Orleans.KurrentDB.StreamProvider` meter reports `continuum.queue.receiver.state`, `continuum.queue.receiver.reconnects`, and `continuum.queue.receiver.initialization.failures`. These are tagged with the stream provider name and queue name. The standard Orleans streaming receiver, cache, and block pool metrics are still reported as before.
