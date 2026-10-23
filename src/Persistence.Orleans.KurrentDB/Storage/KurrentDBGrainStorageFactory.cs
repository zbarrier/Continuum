using Continuum.Persistence.Orleans;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Configuration;

namespace Orleans.Storage;

/// <summary>
///     Factory used to create instances of KurrentDB grain storage.
/// </summary>
public static class KurrentDBGrainStorageFactory
{
    /// <summary>
    ///     Creates a KurrentDB grain storage instance.
    /// </summary>
    public static KurrentDBGrainStorage Create(IServiceProvider serviceProvider, string name)
    {
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<KurrentDBGrainStorageOptions>>();
        return ActivatorUtilities.CreateInstance<KurrentDBGrainStorage>(serviceProvider, name, options.Get(name));
    }

    /// <summary>
    ///     Creates a grain storage that routes each grain to the storage for its tenant.
    /// </summary>
    /// <remarks>
    ///     A tenant's options start from a fresh copy of the provider's options, then
    ///     <paramref name="configureTenantOptions"/> is applied. Tenants left unconfigured use the provider's settings.
    /// </remarks>
    public static TenantGrainStorage CreateMultitenant(IServiceProvider serviceProvider, string name,
        Action<string, KurrentDBGrainStorageOptions> configureTenantOptions)
    {
        ArgumentNullException.ThrowIfNull(configureTenantOptions);
        var defaultStorage = Create(serviceProvider, name);
        return new TenantGrainStorage(defaultStorage, async (tenantId, cancellationToken) =>
        {
            var options = CreateTenantOptions(serviceProvider, name, tenantId, configureTenantOptions);
            var storage = ActivatorUtilities.CreateInstance<KurrentDBGrainStorage>(serviceProvider, name, options);
            await storage.InitializeAsync(cancellationToken).ConfigureAwait(false);
            return storage;
        });
    }

    private static KurrentDBGrainStorageOptions CreateTenantOptions(IServiceProvider serviceProvider, string name, string tenantId,
        Action<string, KurrentDBGrainStorageOptions> configureTenantOptions)
    {
        var options = serviceProvider.GetRequiredService<IOptionsFactory<KurrentDBGrainStorageOptions>>().Create(name);
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
        new DefaultKurrentDBGrainStorageOptionsConfigurator(serviceProvider).PostConfigure(name, options);
        new KurrentDBGrainStorageOptionsValidator(options, $"{name} (tenant {tenantId})").ValidateConfiguration();
        return options;
    }
}
