using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Diagnostics;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Registers the KurrentDB stream provider queue receivers health check.
/// </summary>
public static class KurrentDBQueueReceiversHealthChecksBuilderExtensions
{
    /// <summary>
    ///     Adds a health check reporting the state of every KurrentDB stream provider queue receiver on the silo, for
    ///     both catch-up and persistent subscriptions.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The health check name.</param>
    /// <param name="failureStatus">The status reported when a receiver has faulted.</param>
    /// <param name="tags">Tags for filtering the health check.</param>
    /// <returns>The health checks builder.</returns>
    public static IHealthChecksBuilder AddKurrentDBStreamProviderReceivers(this IHealthChecksBuilder builder,
        string name = "kurrentdb-stream-provider-receivers",
        HealthStatus failureStatus = HealthStatus.Unhealthy,
        IEnumerable<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddSingleton<KurrentDBQueueReceiverStatusRegistry>();
        return builder.Add(new HealthCheckRegistration(name,
            sp => new KurrentDBQueueReceiversHealthCheck(sp.GetRequiredService<KurrentDBQueueReceiverStatusRegistry>()),
            failureStatus, tags));
    }
}
