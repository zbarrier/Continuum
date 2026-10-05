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
}
