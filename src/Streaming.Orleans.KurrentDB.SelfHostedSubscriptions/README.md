# Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions

[KurrentDB](https://www.kurrent.io/) subscriptions that you host yourself in a worker service or process. Your process runs the subscription and stores its own checkpoints. If you want Orleans to run the subscription and deliver events through Orleans streams, use `Continuum.Streaming.Orleans.KurrentDB.StreamProvider` instead.

Only catch-up subscriptions are supported. KurrentDB persistent subscriptions (server-managed consumer groups) are not currently supported in this package.

## Install

```
dotnet add package Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions
```

## Use

Each subscription reads exactly one source. Pick the base class and matching registration method for it, handle each batch in `OnNextBatchAsync`, and register a checkpoint store such as `AddKurrentDBCheckpointStore`.

| Source | Derive from | Register with |
| --- | --- | --- |
| `$all`, optionally filtered by overriding `Filter` (any KurrentDB `IEventFilter`) | `KurrentDBAllStreamCatchupSubscription<TSubscription, TOptions>` | `AddKurrentDBAllStreamCatchupSubscription(MySubscription.Info)` |
| One named stream, such as `$ce-{Category}` or a custom projection stream, by overriding `StreamName` | `KurrentDBStreamCatchupSubscription<TSubscription, TOptions>` | `AddKurrentDBStreamCatchupSubscription(MySubscription.Info)` |

- `$all` subscriptions checkpoint the `$all` commit position and deliver events appended in one transaction in the same batch.
- `$all` subscriptions exclude `$`-prefixed system events (links, metadata, checkpoints) by default with `EventTypeFilter.ExcludeSystemEvents()`. Overriding `Filter` replaces that default; prefer prefix filters over regular expressions, and return `null` only to read every event.
- Stream subscriptions checkpoint the stream revision and must not name `$all`.
- Link events are resolved to the events they point at (`ResolveLinkTos`, on by default), which `$ce-` and other projected link streams need.
- With no stored checkpoint, a subscription reads from the beginning of its source. Set `StartFromNow` to start at the latest event instead; that position is saved as the checkpoint right away, and a stream that doesn't exist yet is read from its first event.
- An event whose type can't be mapped stops the subscription (`UnknownEventTypeBehavior.Halt`, the default) without saving a checkpoint past it. Set `UnknownEventTypeBehavior.Skip` to log and continue. For `$all`, use `Filter` to read only the event types the subscription handles.

Options are bound from `Continuum:KurrentDB:Subscriptions:{Name}`, where `{Name}` is the subscription name. Set `Enabled` to `false` to keep a registered subscription from starting.

```json
{
  "Continuum": {
    "KurrentDB": {
      "Subscriptions": {
        "Projections-V1": {
          "Enabled": true,
          "ConnectionName": "kurrentdb",
          "StartFromNow": false
        }
      }
    }
  }
}
```

## Event size

- **Events must not exceed 1 MB**, data and metadata combined. This is KurrentDB's default maximum append size.
- **Keep metadata under 16 KB.** Metadata is carried with every event, so use it for small values such as correlation and causation ids, tenant and user details and tracing headers, and keep the payload in the event data.

## Monitor

Add a worker-wide health check with `builder.Services.AddHealthChecks().AddKurrentDBCatchupSubscriptions()`. It reports Unhealthy when any subscription has stopped on a batch it could not process (it stays stopped until the worker is fixed and restarted), Degraded while any subscription is reconnecting or retrying a batch, and Healthy otherwise.

Metrics are recorded with `System.Diagnostics.Metrics` on the meter `KurrentDBCatchupSubscriptionMetrics.MeterName` (`Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions`), tagged with `continuum.subscription.name`: events and batches processed, batch retries, reconnects, failures, batch duration and a subscription state gauge. To export them with OpenTelemetry, add the meter: `.WithMetrics(m => m.AddMeter(KurrentDBCatchupSubscriptionMetrics.MeterName))`.

## Planned

- **OpenTelemetry tracing.** Batch processing will be traced from the trace context each event carries in its metadata. A batch holding a single transaction continues that transaction's trace; a batch spanning multiple transactions starts its own activity with span links to each transaction's trace context.
