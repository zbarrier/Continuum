using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Diagnostics;

/// <summary>
///     Reports the health of every catch-up subscription in the process.
/// </summary>
/// <remarks>
///     The change feed processor retries a failed batch until it succeeds, so a subscription never stops on its own.
///     The check reports the registration's failure status while any subscription is retrying a batch. Otherwise healthy.
/// </remarks>
public sealed class CosmosDBCatchupSubscriptionsHealthCheck : IHealthCheck
{
    private readonly CosmosDBCatchupSubscriptionStatusRegistry _statusRegistry;

    /// <summary>
    ///     Initializes a new instance of the health check.
    /// </summary>
    /// <param name="statusRegistry">The registry subscription statuses are read from.</param>
    public CosmosDBCatchupSubscriptionsHealthCheck(CosmosDBCatchupSubscriptionStatusRegistry statusRegistry)
    {
        ArgumentNullException.ThrowIfNull(statusRegistry);
        _statusRegistry = statusRegistry;
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var statuses = _statusRegistry.Statuses;
        var data = statuses.ToDictionary(s => s.SubscriptionName, s => (object)s.State.ToString(), StringComparer.Ordinal);

        var retrying = statuses.Where(s => s.State == CosmosDBCatchupSubscriptionState.Retrying).ToList();
        if (retrying.Count > 0)
        {
            var description = string.Join(" ", retrying.Select(s => $"Subscription '{s.SubscriptionName}' is retrying: {s.Description}"));
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description, retrying[0].Exception, data));
        }

        return Task.FromResult(HealthCheckResult.Healthy($"{statuses.Count} catch-up subscriptions are healthy.", data));
    }
}
