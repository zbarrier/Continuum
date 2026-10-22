using System.Diagnostics.Metrics;

using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Diagnostics;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Tests;

/// <summary>
/// Verifies that the health check reflects the worst subscription state and that metrics are published under the
/// documented meter.
/// </summary>
public class KurrentDBCatchupSubscriptionDiagnosticsTests
{
    private static Task<HealthCheckResult> CheckAsync(KurrentDBCatchupSubscriptionStatusRegistry registry)
    {
        var healthCheck = new KurrentDBCatchupSubscriptionsHealthCheck(registry);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, null)
        };
        return healthCheck.CheckHealthAsync(context, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Is_healthy_when_every_subscription_is_running()
    {
        var registry = new KurrentDBCatchupSubscriptionStatusRegistry();
        registry.Report(new("Orders", KurrentDBCatchupSubscriptionState.Running));
        registry.Report(new("Invoices", KurrentDBCatchupSubscriptionState.Running));

        var result = await CheckAsync(registry);

        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Equal("Running", result.Data["Orders"]);
    }

    [Theory]
    [InlineData(KurrentDBCatchupSubscriptionState.Reconnecting)]
    [InlineData(KurrentDBCatchupSubscriptionState.Retrying)]
    public async Task Is_degraded_while_a_subscription_is_recovering(KurrentDBCatchupSubscriptionState state)
    {
        var registry = new KurrentDBCatchupSubscriptionStatusRegistry();
        registry.Report(new("Orders", KurrentDBCatchupSubscriptionState.Running));
        registry.Report(new("Invoices", state, "waiting"));

        var result = await CheckAsync(registry);

        Assert.Equal(HealthStatus.Degraded, result.Status);
        Assert.Contains("Invoices", result.Description);
    }

    [Fact]
    public async Task Is_unhealthy_when_any_subscription_has_faulted()
    {
        var registry = new KurrentDBCatchupSubscriptionStatusRegistry();
        var error = new InvalidOperationException("bad data");
        registry.Report(new("Orders", KurrentDBCatchupSubscriptionState.Retrying));
        registry.Report(new("Invoices", KurrentDBCatchupSubscriptionState.Faulted, "batch from 10 to 20", error));

        var result = await CheckAsync(registry);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("Invoices", result.Description);
        Assert.Contains("batch from 10 to 20", result.Description);
        Assert.Same(error, result.Exception);
    }

    [Fact]
    public void Publishes_counters_and_state_gauge_tagged_with_the_subscription_name()
    {
        var registry = new KurrentDBCatchupSubscriptionStatusRegistry();
        registry.Report(new("Orders", KurrentDBCatchupSubscriptionState.Faulted));
        using var metrics = new KurrentDBCatchupSubscriptionMetrics(registry);
        var recorded = new List<(string Name, long Value, object? Subscription)>();
        var states = new List<(long Value, object? Subscription, object? State)>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == KurrentDBCatchupSubscriptionMetrics.MeterName)
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
        metrics.Reconnected("Orders");
        metrics.Failed("Orders");
        listener.RecordObservableInstruments();

        Assert.Contains(("continuum.subscription.events.processed", 3L, (object?)"Orders"), recorded);
        Assert.Contains(("continuum.subscription.batches.processed", 1L, (object?)"Orders"), recorded);
        Assert.Contains(("continuum.subscription.batch.retries", 1L, (object?)"Orders"), recorded);
        Assert.Contains(("continuum.subscription.reconnects", 1L, (object?)"Orders"), recorded);
        Assert.Contains(("continuum.subscription.failures", 1L, (object?)"Orders"), recorded);
        Assert.Contains((1L, (object?)"Orders", (object?)"faulted"), states);
        Assert.Contains((0L, (object?)"Orders", (object?)"running"), states);
        Assert.Single(states, state => state.Value == 1 && Equals(state.Subscription, "Orders"));
        Assert.Equal(["kurrentdb"], dbSystems);
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
