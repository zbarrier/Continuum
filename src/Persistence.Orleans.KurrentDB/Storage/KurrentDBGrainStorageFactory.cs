using Continuum.Orleans.Tenancy;
using Microsoft.Extensions.Logging;
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
        }, CreateFailureLogger(serviceProvider, name));

    }

    private static TenantStorageFailureLogger CreateFailureLogger(IServiceProvider serviceProvider, string name) =>
        new(serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<TenantStorageFailureLogger>(), name,
            serviceProvider.GetService<TimeProvider>());

    private static KurrentDBGrainStorageOptions CreateTenantOptions(IServiceProvider serviceProvider, string name, string tenantId,
        Action<string, KurrentDBGrainStorageOptions> configureTenantOptions)
    {
        var options = serviceProvider.GetRequiredService<IOptionsFactory<KurrentDBGrainStorageOptions>>().Create(name);
        var typeMapperKey = options.TypeMapperKey;
        var serializer = options.GrainStorageSerializer;
        var typeMapper = options.TypeMapper;
        configureTenantOptions(tenantId, options);
        if (!string.Equals(typeMapperKey, options.TypeMapperKey, StringComparison.Ordinal))
        {
            // The serializer and type mapper depend only on the type mapper key, not the connection, so they are
            // resolved again only when the tenant callback changes the key without setting them itself.
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
