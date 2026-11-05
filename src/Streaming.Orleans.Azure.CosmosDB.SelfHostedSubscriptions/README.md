# Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions

Azure Cosmos DB change feed subscriptions that you host yourself in a worker service or process. Your process runs the subscription; the Cosmos DB change feed processor stores progress in a lease container.

The subscriptions read the change feed of a container written by `Continuum.EventSourcing.Orleans.CosmosDB`.

## Install

```
dotnet add package Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions
```

## Use

Register it with `AddCosmosDBCatchupSubscription`. Options bind from `Continuum:CosmosDB:Subscriptions:{SubscriptionName}`, so a subscription named `Projections-V1` reads its settings from `Continuum:CosmosDB:Subscriptions:Projections-V1`. Values set in code with `Configure` are applied after configuration binding.

```json
{
  "Continuum": {
	"CosmosDB": {
	  "Subscriptions": {
		"Projections-V1": {
		  "Enabled": true,
		  "ConnectionName": "EventStore",
		  "DatabaseName": "continuum",
		  "MonitoredContainerName": "events",
		  "LeaseContainerName": "leases",
		  "InstanceName": "worker-1",
		  "StartFrom": "Beginning",
		  "PollingInterval": "00:00:05",
		  "MaxItemsPerPoll": 500,
		  "UnknownEventTypeBehavior": "Halt"
		}
	  }
	}
  }
}
```

### Options

| Option | Default | Description |
| --- | --- | --- |
| `Enabled` | `true` | When `false`, the subscription is registered but does not start. |
| `ConnectionName` | (required) | Name of the keyed `CosmosClient`, serializer, and type mapper to use. |
| `DatabaseName` | (required) | Database that holds the monitored and lease containers. |
| `MonitoredContainerName` | (required) | Container whose change feed is read (the event store container). |
| `LeaseContainerName` | (required) | Container where the change feed processor stores leases (progress). |
| `InstanceName` | (required) | Unique name of this host. Instances with the same subscription name share the leases and split the work. |
| `StartFrom` | `Beginning` | `Beginning`, `Now`, or `DateTime`. |
| `StartFromDateTime` | `null` | Start time when `StartFrom` is `DateTime`; required in that case. |
| `PollingInterval` | `00:00:05` | Delay between change feed polls. Must be between 100 milliseconds and 1 minute. |
| `MaxItemsPerPoll` | `500` | Maximum number of items read per poll. Must be between 1 and 10,000. |
| `UnknownEventTypeBehavior` | `Halt` | What to do with an event whose type the type mapper cannot resolve. See below. |

### Starting position

The starting position applies only when no leases exist yet for the subscription. Once leases exist, the subscription always resumes from the stored leases, so changing `StartFrom` later has no effect. To reprocess from a new position, use a new subscription name (for example `Projections-V2`) or delete its leases.

### Unknown event types

With `Halt` (the default), an event whose type isn't mapped fails the batch. The change feed processor retries the lease without moving past the event, and the subscription reports `Retrying` in health checks and metrics until you deploy a type mapping for it. Choose `Skip` only when the container is expected to hold events this subscription can't read; skipped events are logged as warnings and are not delivered.

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
