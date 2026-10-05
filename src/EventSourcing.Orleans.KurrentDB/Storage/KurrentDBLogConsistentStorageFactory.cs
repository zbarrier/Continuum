using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Orleans.Configuration;

namespace Continuum.EventSourcing.Orleans.KurrentDB;

/// <summary>
///     Factory used to create instances of Kurrent log consistent storage.
/// </summary>
public static class KurrentDBLogConsistentStorageFactory
{
    /// <summary>
    ///     Creates an KurrentDB log consistent storage instance.
    /// </summary>
    public static KurrentDBLogConsistentStorage Create(IServiceProvider serviceProvider, string name)
    {
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<KurrentDBLogConsistentStorageOptions>>();
        return new KurrentDBLogConsistentStorage(serviceProvider,
            name,
            options.Get(name),
            serviceProvider.GetRequiredService<IOptions<ClusterOptions>>(),
            serviceProvider.GetRequiredService<ILogger<KurrentDBLogConsistentStorage>>());
    }
}
