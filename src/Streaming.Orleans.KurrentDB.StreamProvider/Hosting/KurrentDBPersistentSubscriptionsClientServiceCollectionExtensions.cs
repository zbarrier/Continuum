using KurrentDB.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Orleans.Hosting;

/// <summary>
///     Extension methods that register a keyed <see cref="KurrentDBPersistentSubscriptionsClient" />.
/// </summary>
/// <remarks>
///     The Aspire <c>AddKeyedKurrentDBClient</c> integration only registers a <see cref="KurrentDBClient" />, and a
///     <see cref="KurrentDBClient" /> does not expose the settings it was built from, so a persistent subscriptions
///     client cannot be derived from it. These helpers register one alongside it using the same connection name.
/// </remarks>
public static class KurrentDBPersistentSubscriptionsClientServiceCollectionExtensions
{
    /// <summary>
    ///     Registers a keyed <see cref="KurrentDBPersistentSubscriptionsClient" /> built from the connection string
    ///     with the supplied <paramref name="connectionName" />.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionName">The connection name used as the service key and configuration lookup.</param>
    public static IServiceCollection AddKeyedKurrentDBPersistentSubscriptionsClient(this IServiceCollection services, string connectionName)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName, nameof(connectionName));
        services.AddKeyedSingleton(connectionName, (provider, _) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString(connectionName)
                                   ?? throw new InvalidOperationException($"No connection string named \"{connectionName}\" was found. It is required to create a {nameof(KurrentDBPersistentSubscriptionsClient)}.");
            return new KurrentDBPersistentSubscriptionsClient(KurrentDBClientSettings.Create(connectionString));
        });
        return services;
    }

    /// <summary>
    ///     Registers a keyed <see cref="KurrentDBPersistentSubscriptionsClient" /> built from the supplied settings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionName">The connection name used as the service key.</param>
    /// <param name="settings">The client settings to build the client from.</param>
    public static IServiceCollection AddKeyedKurrentDBPersistentSubscriptionsClient(this IServiceCollection services, string connectionName, KurrentDBClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName, nameof(connectionName));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        services.AddKeyedSingleton(connectionName, (_, _) => new KurrentDBPersistentSubscriptionsClient(settings));
        return services;
    }
}
