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
}
