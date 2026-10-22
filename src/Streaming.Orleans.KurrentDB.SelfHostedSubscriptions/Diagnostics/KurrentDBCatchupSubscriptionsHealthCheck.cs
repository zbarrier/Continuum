using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Diagnostics;

/// <summary>
///     Reports the health of every catch-up subscription in the process.
/// </summary>
/// <remarks>
///     Unhealthy when any subscription has faulted, since it stays stopped until the worker is fixed and restarted.
///     Degraded while any subscription is reconnecting or retrying a batch. Otherwise healthy.
/// </remarks>
public sealed class KurrentDBCatchupSubscriptionsHealthCheck : IHealthCheck
{
    private readonly KurrentDBCatchupSubscriptionStatusRegistry _statusRegistry;

    /// <summary>
    ///     Initializes a new instance of the health check.
    /// </summary>
    /// <param name="statusRegistry">The registry subscription statuses are read from.</param>
    public KurrentDBCatchupSubscriptionsHealthCheck(KurrentDBCatchupSubscriptionStatusRegistry statusRegistry)
    {
        ArgumentNullException.ThrowIfNull(statusRegistry);
        _statusRegistry = statusRegistry;
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var statuses = _statusRegistry.Statuses;
        var data = statuses.ToDictionary(s => s.SubscriptionName, s => (object)s.State.ToString(), StringComparer.Ordinal);

        var faulted = statuses.Where(s => s.State == KurrentDBCatchupSubscriptionState.Faulted).ToList();
        if (faulted.Count > 0)
        {
            var description = string.Join(" ", faulted.Select(s => $"Subscription '{s.SubscriptionName}' faulted: {s.Description}"));
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description, faulted[0].Exception, data));
        }

        var degraded = statuses.Where(s => s.State is KurrentDBCatchupSubscriptionState.Reconnecting or KurrentDBCatchupSubscriptionState.Retrying).ToList();
        if (degraded.Count > 0)
        {
            var description = string.Join(" ", degraded.Select(s => $"Subscription '{s.SubscriptionName}' is {s.State.ToString().ToLowerInvariant()}: {s.Description}"));
            return Task.FromResult(HealthCheckResult.Degraded(description, degraded[0].Exception, data));
        }

        return Task.FromResult(HealthCheckResult.Healthy($"{statuses.Count} catch-up subscriptions are healthy.", data));
    }
}
