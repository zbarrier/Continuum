using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Diagnostics;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Registers the Cosmos DB catch-up subscriptions health check.
/// </summary>
public static class CosmosDBCatchupSubscriptionsHealthChecksBuilderExtensions
{
    /// <summary>
    ///     Adds a health check reporting the state of every Cosmos DB catch-up subscription in the process.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The health check name.</param>
    /// <param name="failureStatus">The status reported while a subscription is retrying a failed batch.</param>
    /// <param name="tags">Tags for filtering the health check.</param>
    /// <returns>The health checks builder.</returns>
    public static IHealthChecksBuilder AddCosmosDBCatchupSubscriptions(this IHealthChecksBuilder builder,
        string name = "cosmosdb-catchup-subscriptions",
        HealthStatus failureStatus = HealthStatus.Degraded,
        IEnumerable<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddSingleton<CosmosDBCatchupSubscriptionStatusRegistry>();
        return builder.Add(new HealthCheckRegistration(name,
            sp => new CosmosDBCatchupSubscriptionsHealthCheck(sp.GetRequiredService<CosmosDBCatchupSubscriptionStatusRegistry>()),
            failureStatus, tags));
    }
}
