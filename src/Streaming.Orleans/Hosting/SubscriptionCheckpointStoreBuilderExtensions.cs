using Continuum.Streaming;
using Continuum.Streaming.Orleans;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Continuum.Hosting.Orleans;

/// <summary>
/// Registers <see cref="SubscriptionCheckpointStore{TPosition}"/> with dependency injection.
/// </summary>
/// <remarks>
/// A grain storage provider must also be registered under <see cref="SubscriptionCheckpointGrain.StorageName"/>.
/// </remarks>
public static class SubscriptionCheckpointStoreBuilderExtensions
{
    /// <summary>
    /// Registers a keyed grain-backed <see cref="ICheckpointStore{TStreamPosition}"/> for <paramref name="connectionName"/>.
    /// </summary>
    /// <typeparam name="TPosition">The provider-specific stream position type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionName">The service key for the checkpoint store.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddSubscriptionCheckpointStore<TPosition>(this IServiceCollection services, string connectionName)
        where TPosition : IComparable<TPosition>
    {
        services.AddKeyedSingleton<ICheckpointStore<TPosition>>(connectionName,
            (sp, _) => new SubscriptionCheckpointStore<TPosition>(sp.GetRequiredService<IGrainFactory>()));
        return services;
    }

    /// <inheritdoc cref="AddSubscriptionCheckpointStore{TPosition}(IServiceCollection, string)"/>
    /// <param name="builder">The host application builder.</param>
    /// <param name="connectionName">The service key for the checkpoint store.</param>
    /// <returns>The host application builder.</returns>
    public static IHostApplicationBuilder AddSubscriptionCheckpointStore<TPosition>(this IHostApplicationBuilder builder, string connectionName)
        where TPosition : IComparable<TPosition>
    {
        _ = builder.Services.AddSubscriptionCheckpointStore<TPosition>(connectionName);
        return builder;
    }
}
