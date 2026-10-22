using System.Diagnostics.Metrics;

using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Diagnostics;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Tests;

/// <summary>
/// Verifies that the health check reflects retrying subscriptions and that metrics are published under the
/// documented meter.
/// </summary>
public class CosmosDBCatchupSubscriptionDiagnosticsTests
{
    private static Task<HealthCheckResult> CheckAsync(CosmosDBCatchupSubscriptionStatusRegistry registry,
        HealthStatus failureStatus = HealthStatus.Degraded)
    {
        var healthCheck = new CosmosDBCatchupSubscriptionsHealthCheck(registry);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("test", healthCheck, failureStatus, null)
        };
        return healthCheck.CheckHealthAsync(context, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Is_healthy_when_every_subscription_is_running()
    {
        var registry = new CosmosDBCatchupSubscriptionStatusRegistry();
        registry.Report(new("Orders", CosmosDBCatchupSubscriptionState.Running));
        registry.Report(new("Invoices", CosmosDBCatchupSubscriptionState.Running));

        var result = await CheckAsync(registry);

        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Equal("Running", result.Data["Orders"]);
    }

    [Theory]
    [InlineData(HealthStatus.Degraded)]
    [InlineData(HealthStatus.Unhealthy)]
    public async Task Reports_the_failure_status_while_a_subscription_is_retrying(HealthStatus failureStatus)
    {
        var registry = new CosmosDBCatchupSubscriptionStatusRegistry();
        var error = new InvalidOperationException("bad data");
        registry.Report(new("Orders", CosmosDBCatchupSubscriptionState.Running));
        registry.Report(new("Invoices", CosmosDBCatchupSubscriptionState.Retrying, "Lease 0 failed to handle a batch.", error));

        var result = await CheckAsync(registry, failureStatus);

        Assert.Equal(failureStatus, result.Status);
        Assert.Contains("Invoices", result.Description);
        Assert.Contains("Lease 0", result.Description);
        Assert.Same(error, result.Exception);
    }

    [Fact]
    public void Publishes_counters_and_state_gauge_tagged_with_the_subscription_name()
    {
        var registry = new CosmosDBCatchupSubscriptionStatusRegistry();
        registry.Report(new("Orders", CosmosDBCatchupSubscriptionState.Retrying));
        using var metrics = new CosmosDBCatchupSubscriptionMetrics(registry);
        var recorded = new List<(string Name, long Value, object? Subscription)>();
        var states = new List<(long Value, object? Subscription, object? State)>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == CosmosDBCatchupSubscriptionMetrics.MeterName)
            {
                l.EnableMeasurementEvents(instrument);
            }
        };
        var dbSystems = new HashSet<object?>();
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
        {
            dbSystems.Add(Tag(tags, "db.system.name"));
            recorded.Add((instrument.Name, value, Tag(tags, "continuum.subscription.name")));
        });
        listener.SetMeasurementEventCallback<double>((_, _, tags, _) => dbSystems.Add(Tag(tags, "db.system.name")));
        listener.SetMeasurementEventCallback<int>((_, value, tags, _) =>
        {
            dbSystems.Add(Tag(tags, "db.system.name"));
            states.Add((value, Tag(tags, "continuum.subscription.name"), Tag(tags, "continuum.subscription.state")));
        });
        listener.Start();

        metrics.BatchProcessed("Orders", 3, TimeSpan.FromMilliseconds(5));
        metrics.BatchRetried("Orders");
        metrics.Error("Orders");
        listener.RecordObservableInstruments();

        Assert.Contains(("continuum.subscription.events.processed", 3L, (object?)"Orders"), recorded);
        Assert.Contains(("continuum.subscription.batches.processed", 1L, (object?)"Orders"), recorded);
        Assert.Contains(("continuum.subscription.batch.retries", 1L, (object?)"Orders"), recorded);
        Assert.Contains(("continuum.subscription.errors", 1L, (object?)"Orders"), recorded);
        Assert.Contains((1L, (object?)"Orders", (object?)"retrying"), states);
        Assert.Contains((0L, (object?)"Orders", (object?)"running"), states);
        Assert.Single(states, state => state.Value == 1 && Equals(state.Subscription, "Orders"));
        Assert.Equal(["azure.cosmosdb"], dbSystems);
    }

    private static object? Tag(ReadOnlySpan<KeyValuePair<string, object?>> tags, string key)
    {
        foreach (var tag in tags)
        {
            if (tag.Key == key)
            {
                return tag.Value;
            }
        }
        return null;
    }
}
