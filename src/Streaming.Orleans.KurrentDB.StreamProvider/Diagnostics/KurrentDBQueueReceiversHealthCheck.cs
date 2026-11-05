using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Diagnostics;

/// <summary>
///     Reports the health of the KurrentDB stream provider queue receivers running on this silo.
/// </summary>
/// <remarks>
///     Unhealthy when a receiver failed to start, Degraded while a receiver is starting or reconnecting, and Healthy
///     otherwise. Failures delivering events to grains are not receiver failures and are not reported.
/// </remarks>
public sealed class KurrentDBQueueReceiversHealthCheck : IHealthCheck
{
    private readonly KurrentDBQueueReceiverStatusRegistry _statusRegistry;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBQueueReceiversHealthCheck" /> class.
    /// </summary>
    public KurrentDBQueueReceiversHealthCheck(KurrentDBQueueReceiverStatusRegistry statusRegistry)
    {
        ArgumentNullException.ThrowIfNull(statusRegistry);
        _statusRegistry = statusRegistry;
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var statuses = _statusRegistry.Statuses;
        var data = statuses.ToDictionary(s => $"{s.ProviderName}/{s.QueueName}", s => (object)s.State.ToString(), StringComparer.Ordinal);

        var faulted = statuses.Where(s => s.State == KurrentDBQueueReceiverState.Faulted).ToList();
        if (faulted.Count > 0)
        {
            var description = string.Join(" ", faulted.Select(s => $"Queue '{s.QueueName}' of provider '{s.ProviderName}' faulted: {s.Description}"));
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description, faulted[0].Exception, data));
        }

        var degraded = statuses.Where(s => s.State is KurrentDBQueueReceiverState.Starting or KurrentDBQueueReceiverState.Reconnecting).ToList();
        if (degraded.Count > 0)
        {
            var description = string.Join(" ", degraded.Select(s => $"Queue '{s.QueueName}' of provider '{s.ProviderName}' is {s.State.ToString().ToLowerInvariant()}."));
            return Task.FromResult(HealthCheckResult.Degraded(description, degraded[0].Exception, data));
        }

        return Task.FromResult(HealthCheckResult.Healthy($"{statuses.Count} queue receivers are healthy.", data));
    }
}
