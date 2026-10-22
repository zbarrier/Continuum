# Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions

Azure Cosmos DB change feed subscriptions that you host yourself in a worker service or process. Your process runs the subscription; the Cosmos DB change feed processor stores progress in a lease container.

The subscriptions read the change feed of a container written by `Continuum.EventSourcing.Orleans.CosmosDB`.

## Install

```
dotnet add package Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions
```

## Use

Register it with `AddCosmosDBCatchupSubscription`.

## Health checks

The change feed processor retries a batch that fails in your handler until it succeeds, so a subscription never stops on its own. Add a health check that reports `Degraded` (or the failure status you choose) while any subscription is retrying a failed batch:

```csharp
builder.Services.AddHealthChecks().AddCosmosDBCatchupSubscriptions();
```

## Metrics

Subscriptions record metrics with the meter named by `CosmosDBCatchupSubscriptionMetrics.MeterName`. Collect them with OpenTelemetry:

```csharp
builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics.AddMeter(CosmosDBCatchupSubscriptionMetrics.MeterName));
```

| Instrument | Description |
| --- | --- |
| `continuum.subscription.events.processed` | Events handled successfully. |
| `continuum.subscription.batches.processed` | Batches handled successfully. |
| `continuum.subscription.batch.duration` | Time taken to handle a batch successfully, in seconds. |
| `continuum.subscription.batch.retries` | Batches that failed in the handler and will be retried. |
| `continuum.subscription.errors` | Change feed processor errors not raised by the handler, such as lost leases. |
| `continuum.subscription.state` | 0 starting, 1 running, 2 retrying, 3 stopped. |

Every measurement is tagged with `continuum.subscription.name`.
