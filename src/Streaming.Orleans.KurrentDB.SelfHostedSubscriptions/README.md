# Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions

[KurrentDB](https://www.kurrent.io/) subscriptions that you host yourself in a worker service or process. Your process runs the subscription and stores its own checkpoints. If you want Orleans to run the subscription and deliver events through Orleans streams, use `Continuum.Streaming.Orleans.KurrentDB.StreamProvider` instead.

Only catch-up subscriptions are supported. KurrentDB persistent subscriptions (server-managed consumer groups) are not currently supported in this package.

## Install

```
dotnet add package Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions
```

## Use

Derive from `KurrentDBCatchupSubscription<TSubscription, TOptions>`, supply the stream to read (`KurrentDBStreams.All` for `$all`) and an optional filter, and handle each batch in `OnNextBatchAsync`. Register it as a hosted service with `AddKurrentDBCatchupSubscription`, and register a checkpoint store such as `AddKurrentDBCheckpointStore`.

## Event size

- **Events must not exceed 1 MB**, data and metadata combined. This is KurrentDB's default maximum append size.
- **Keep metadata under 16 KB.** Metadata is carried with every event, so use it for small values such as correlation and causation ids, tenant and user details and tracing headers, and keep the payload in the event data.

## Monitor

Add a worker-wide health check with `builder.Services.AddHealthChecks().AddKurrentDBCatchupSubscriptions()`. It reports Unhealthy when any subscription has stopped on a batch it could not process (it stays stopped until the worker is fixed and restarted), Degraded while any subscription is reconnecting or retrying a batch, and Healthy otherwise.

Metrics are recorded with `System.Diagnostics.Metrics` on the meter `KurrentDBCatchupSubscriptionMetrics.MeterName` (`Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions`), tagged with `continuum.subscription.name`: events and batches processed, batch retries, reconnects, failures, batch duration and a subscription state gauge. To export them with OpenTelemetry, add the meter: `.WithMetrics(m => m.AddMeter(KurrentDBCatchupSubscriptionMetrics.MeterName))`.
