# KurrentDB subscription parity: self-hosted vs. Orleans stream provider

This list tracks feature differences between `Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions` (self-hosted) and `Continuum.Streaming.Orleans.KurrentDB.StreamProvider` (provider). Some differences are on purpose because the two are built differently: self-hosted runs its own batching, retries and state, while the provider leaves delivery and retries to Orleans.

## Done

| Feature | Notes |
| --- | --- |
| Credentials per subscription | Self-hosted sets `Credentials` on each subscription. A provider is one subscription, and its `KurrentDBOptions.Credentials` is set separately for each stream provider, even when providers share a connection. Both use `KurrentDBSubscriptionCredentialsOptions` from the Abstractions project. |
| Provider health check and metrics | Each catch-up and persistent queue receiver reports Starting, Running, Reconnecting, or Faulted (failed to start). The health check is added with `AddHealthChecks().AddKurrentDBStreamProviderReceivers()`. The `Continuum.Streaming.Orleans.KurrentDB.StreamProvider` meter reports receiver state, reconnects, and initialization failures. The Orleans receiver, cache, and block pool monitors are unchanged. A failed delivery is seen by the subscribing grain, not the receiver, so failures for each consumer are not reported. |
| Separate `$all` and named-stream APIs | Both projects. |
| `Catchup` spelling | Both projects. |
| `IEventFilter` filtering, excluding system events by default for `$all` | Both projects. |
| `StartFromNow` | Both catch-up implementations default to `false`, so a subscription with no checkpoint reads its full history. Provider persistent subscriptions default to `true`. Both save the starting position as the checkpoint right away. |
| `ResolveLinkTos` | Both default to `true`. |
| Retry when initialization fails | Self-hosted reads the checkpoint and start position inside its reconnect loop, so failures back off and retry. |
| Skip `$all` events that have no position | Both log a warning and skip the event. |
| Unknown event type behavior (`Halt` / `Skip`) | Both default to `Halt` and use the shared `UnknownEventTypeBehavior` enum from `Continuum.Streaming.Orleans`. |
| Configuring subscriptions from appsettings | The provider registers as Orleans provider type `KurrentDB` under `Orleans:Streaming:{Name}`, like the built-in Orleans stream providers. Adding or removing a section enables or disables that subscription. KurrentDB client types (`ClientSettings`, `SubscriptionSettings`) and the data adapter stay code-only. |

## Missing in self-hosted (provider has it)

None.

## Missing in the provider (self-hosted has it)

None.

## Differences by design

- Orleans queue, cache, and eviction integration (provider only).
- Delivery retries: the provider leaves them to Orleans pub/sub, which retries each delivery and then calls the stream failure handler. Self-hosted batch retries, transient vs. non-transient classification, and the Faulted state would add a second retry policy that conflicts with Orleans'.
- Transaction batching: Orleans delivers one event at a time, and a grain subscribed to several streams gets each stream separately, so events from one `$all` transaction can't be delivered as a unit. The provider keeps `$all` order within each stream. Consumers that must handle a whole transaction at once should use self-hosted subscriptions.
