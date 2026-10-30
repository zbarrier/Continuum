# Continuum.Streaming.Orleans.KurrentDB.StreamProvider

An Orleans persistent stream provider for [KurrentDB](https://www.kurrent.io/). Orleans runs the subscription and delivers events to grains through Orleans streams. Use it when grains, such as projections, should react to events written to KurrentDB.

There are two ways to read from KurrentDB:

- **`$all` catch-up subscription** (recommended). Events are delivered in order, none are dropped, and the read position is checkpointed by the provider.
- **Persistent subscription.** KurrentDB tracks the read position for a consumer group. Use it only for consumers that can tolerate dropped and out-of-order events.

If you want to run the subscription yourself in a worker service, use `Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions` instead.

## Install

```
dotnet add package Continuum.Streaming.Orleans.KurrentDB.StreamProvider
```

## `$all` catch-up subscriptions

```csharp
siloBuilder.AddKurrentDBStreams("KurrentDB", configurator =>
{
	configurator.ConfigureKurrentDB(ob => ob.Configure(options => options.Queues = ["all"]));
	configurator.UseCatchUpSubscription(ob => ob.Configure(options =>
	{
		options.StreamFilterPrefix = "snack-";
		options.StartFromNow = false;
	}));
});
```

### How it works

The provider subscribes to `$all`, so it sees every event in the database in commit order. One subscription runs per provider, so `Queues` must have exactly one entry. Orleans runs that queue's pulling agent on one silo in the cluster and moves it to another silo if that silo stops.

Each event is delivered to the Orleans stream named after the KurrentDB stream it was written to. The name is split on the first `-`, so `snack-123` becomes stream namespace `snack` with key `123`. Keys can contain their own dashes, such as GUIDs. A tenant-qualified name such as `snack-tenantA|123` keeps the tenant in the key. To use a different naming convention, set `StreamIdMapper` on the data adapter options.

Use `StreamFilterPrefix` to have KurrentDB send only the streams you need. Link events are resolved to the events they point to by default (`ResolveLinkTos`).

A resolved link carries the same event as the original, at a later `$all` position, and the catch-up subscription delivers every position it reads. If the filter matches both the original streams and the streams holding links to them (for example, no filter at all, which includes the `$ce-` and `$et-` system projection streams), each event is delivered once per match. Filter to either the original streams (for example `order-`) or the link streams (for example `$ce-order`), not both. Persistent subscriptions drop a redelivered event that is still queued, so this applies to catch-up subscriptions only.

### Checkpoints

The provider stores the `$all` position in an `ICheckpointStore<ulong>`. Set `CheckpointConnectionName` to choose which registered store is used. When there is no checkpoint yet, `StartFromNow` (default `true`) chooses between reading only new events and reading from the beginning of `$all`.

The checkpoint only moves past an event once every active subscriber has read it and it has been purged from the cache, so a restart never skips an event a subscriber has not seen. Delivery is at least once, so after a restart some events may be delivered again.

Writes to the checkpoint store are batched by `CheckpointMonitorOptions`. By default, a checkpoint is written after 10,000 events or 20 seconds, whichever comes first, and once more on shutdown.

`StreamPullingAgentOptions.BatchContainerBatchSize` must stay at 1 (the Orleans default). A larger size lets the pulling agent read past events before delivering them, so the checkpoint could move past events no subscriber received. A larger size fails validation at startup.

A catch-up subscription is read only. Events are written by event sourcing, so writing to the provider throws.

### Multiple `$all` subscriptions

You can run as many `$all` subscriptions against the same database as you need. Each one is its own stream provider with no filter, the same filter, or a different filter. Providers that use the same `ConnectionName` share one `KurrentDBClient`, and each subscription runs as a separate call on that client's connection.

```csharp
siloBuilder.AddKurrentDBStreams("Snacks", configurator =>
{
    configurator.ConfigureKurrentDB(ob => ob.Configure(options =>
    {
        options.ConnectionName = "kurrentdb";
        options.Name = "Snacks";
        options.Queues = ["all"];
    }));
    configurator.UseCatchUpSubscription(ob => ob.Configure(options => options.StreamFilterPrefix = "snack-"));
});

siloBuilder.AddKurrentDBStreams("Reporting", configurator =>
{
    configurator.ConfigureKurrentDB(ob => ob.Configure(options =>
    {
        options.ConnectionName = "kurrentdb";
        options.Name = "Reporting";
        options.Queues = ["all"];
    }));
    configurator.UseCatchUpSubscription();
});
```

Each subscription has its own filter, cache and checkpoint. The checkpoint is keyed by the connection name, the Orleans service id and the stream provider name, so subscriptions never share a position.

A consumer chooses a subscription by its stream provider name, then a stream by namespace and key. The namespace is always qualified with the provider name (see [Stream namespaces](#stream-namespaces)):

```csharp
var stream = this.GetStreamProvider("Snacks").GetStream<IStreamedEvent<object>>(StreamId.Create("Snacks:snack", "123"));
```

When two subscriptions both match a KurrentDB stream, each delivers its events on its own provider and namespace, so subscribers of one never receive the other's deliveries. This also holds for implicit subscriptions.

Each subscription reads `$all` and keeps its own cache, so use `StreamFilterPrefix` to keep each one to the streams it needs.

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

Events are cached in 2 MB blocks, which leaves room for a 1 MB event and the extra space the cache needs to store it. An event that does not fit in a block is logged as an error and skipped. It is not delivered or retried. A catch-up subscription's checkpoint moves past it, and a persistent subscription has already acknowledged it.

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
