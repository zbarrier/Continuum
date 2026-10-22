using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Diagnostics;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Registers the KurrentDB catch-up subscriptions health check.
/// </summary>
public static class KurrentDBCatchupSubscriptionsHealthChecksBuilderExtensions
{
    /// <summary>
    ///     Adds a health check reporting the state of every KurrentDB catch-up subscription in the process.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The health check name.</param>
    /// <param name="failureStatus">The status reported when a subscription has faulted.</param>
    /// <param name="tags">Tags for filtering the health check.</param>
    /// <returns>The health checks builder.</returns>
    public static IHealthChecksBuilder AddKurrentDBCatchupSubscriptions(this IHealthChecksBuilder builder,
        string name = "kurrentdb-catchup-subscriptions",
        HealthStatus failureStatus = HealthStatus.Unhealthy,
        IEnumerable<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddSingleton<KurrentDBCatchupSubscriptionStatusRegistry>();
        return builder.Add(new HealthCheckRegistration(name,
            sp => new KurrentDBCatchupSubscriptionsHealthCheck(sp.GetRequiredService<KurrentDBCatchupSubscriptionStatusRegistry>()),
            failureStatus, tags));
    }
}
