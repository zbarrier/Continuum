using Continuum.Orleans.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Orleans.Configuration;

namespace Continuum.EventSourcing.Orleans.CosmosDB;

/// <summary>
///     Factory used to create instances of CosmosDB log consistent storage.
/// </summary>
public static class CosmosDBLogConsistentStorageFactory
{
    /// <summary>
    ///     Creates an CosmosDB log consistent storage instance.
    /// </summary>
    public static CosmosDBLogConsistentStorage Create(IServiceProvider serviceProvider, string name)
    {
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<CosmosDBLogConsistentStorageOptions>>();
        return new CosmosDBLogConsistentStorage(serviceProvider,
            name,
            options.Get(name),
            serviceProvider.GetRequiredService<IOptions<ClusterOptions>>(),
            serviceProvider.GetRequiredService<ILogger<CosmosDBLogConsistentStorage>>());
    }

    /// <summary>
    ///     Creates a log consistent storage that routes each grain to the storage for its tenant.
    /// </summary>
    /// <remarks>
    ///     A tenant's options start from a fresh copy of the provider's options, then
    ///     <paramref name="configureTenantOptions"/> is applied. Tenants left unconfigured use the provider's settings.
    /// </remarks>
    public static TenantLogConsistentStorage CreateMultitenant(IServiceProvider serviceProvider, string name,
        Action<string, CosmosDBLogConsistentStorageOptions> configureTenantOptions)
    {
        ArgumentNullException.ThrowIfNull(configureTenantOptions);
        var defaultStorage = Create(serviceProvider, name);
        return new TenantLogConsistentStorage(defaultStorage, async (tenantId, cancellationToken) =>
        {
            var options = CreateTenantOptions(serviceProvider, name, tenantId, configureTenantOptions);
            var storage = new CosmosDBLogConsistentStorage(serviceProvider,
                name,
                options,
                serviceProvider.GetRequiredService<IOptions<ClusterOptions>>(),
                serviceProvider.GetRequiredService<ILogger<CosmosDBLogConsistentStorage>>());
            await storage.InitializeAsync(cancellationToken).ConfigureAwait(false);
            return storage;
        }, CreateFailureLogger(serviceProvider, name));

    }

    private static TenantStorageFailureLogger CreateFailureLogger(IServiceProvider serviceProvider, string name) =>
        new(serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<TenantStorageFailureLogger>(), name,
            serviceProvider.GetService<TimeProvider>());

    private static CosmosDBLogConsistentStorageOptions CreateTenantOptions(IServiceProvider serviceProvider, string name, string tenantId,
        Action<string, CosmosDBLogConsistentStorageOptions> configureTenantOptions)
    {
        var options = serviceProvider.GetRequiredService<IOptionsFactory<CosmosDBLogConsistentStorageOptions>>().Create(name);
        var connectionName = options.ConnectionName;
        var serializer = options.GrainStorageSerializer;
        var typeMapper = options.TypeMapper;
        configureTenantOptions(tenantId, options);
        if (!string.Equals(connectionName, options.ConnectionName, StringComparison.Ordinal))
        {
            // Values resolved from the provider's connection must be resolved again for the tenant's connection,
            // unless the tenant callback set them explicitly.
            if (ReferenceEquals(serializer, options.GrainStorageSerializer))
            {
                options.GrainStorageSerializer = null!;
            }
            if (ReferenceEquals(typeMapper, options.TypeMapper))
            {
                options.TypeMapper = null!;
            }
        }
        new DefaultCosmosDBLogConsistentStorageOptionsConfigurator(serviceProvider).PostConfigure(name, options);
        new CosmosDBLogConsistentStorageOptionsValidator(options, $"{name} (tenant {tenantId})").ValidateConfiguration();
        return options;
    }
}
