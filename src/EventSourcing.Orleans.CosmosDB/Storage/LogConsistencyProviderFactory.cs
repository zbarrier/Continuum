using Microsoft.Extensions.DependencyInjection;

using Orleans.Serialization;

namespace Continuum.EventSourcing.Orleans.CosmosDB;

/// <summary>
///     Factory used to create instances of log consistent provider.
/// </summary>
public static class LogConsistencyProviderFactory
{
    /// <summary>
    ///     Creates a CosmosDB log consistent storage instance.
    /// </summary>
    public static LogConsistencyProvider Create(IServiceProvider serviceProvider, string name)
    {
        var logConsistentStorage = serviceProvider.GetRequiredKeyedService<ILogConsistentStorage>(name);
        return new LogConsistencyProvider(logConsistentStorage, serviceProvider.GetRequiredService<DeepCopier>());
    }
}
