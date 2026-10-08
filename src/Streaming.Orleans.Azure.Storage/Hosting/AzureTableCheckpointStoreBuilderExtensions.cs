using Azure.Data.Tables;

using Continuum.Streaming;
using Continuum.Streaming.Orleans.Azure.Storage;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Continuum.Orleans.Hosting;

/// <summary>
/// Extension methods for registering <see cref="AzureTableCheckpointStore{TPosition}"/>.
/// </summary>
public static class AzureTableCheckpointStoreBuilderExtensions
{
    /// <summary>
    /// Registers an <see cref="AzureTableCheckpointStore{TPosition}"/> as a keyed
    /// <see cref="ICheckpointStore{TStreamPosition}"/>.
    /// </summary>
    /// <typeparam name="TPosition">The provider-specific stream position type.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="connectionName">
    /// The service key of the registered <see cref="TableServiceClient"/>, also used as the service key of the store.
    /// </param>
    /// <returns>The <paramref name="builder"/>, for chaining.</returns>
    public static IHostApplicationBuilder AddAzureTableCheckpointStore<TPosition>(this IHostApplicationBuilder builder, string connectionName)
        where TPosition : IComparable<TPosition>, ISpanFormattable, IParsable<TPosition>
    {
        builder.Services.AddKeyedSingleton<ICheckpointStore<TPosition>>(connectionName, (provider, _) =>
            new AzureTableCheckpointStore<TPosition>(provider.GetRequiredKeyedService<TableServiceClient>(connectionName)));
        return builder;
    }

    /// <summary>
    /// Registers an <see cref="AzureTableCheckpointStore{TPosition}"/> as a keyed
    /// <see cref="ICheckpointStore{TStreamPosition}"/> and as the default (unkeyed)
    /// <see cref="ICheckpointStore{TStreamPosition}"/>.
    /// </summary>
    /// <typeparam name="TPosition">The provider-specific stream position type.</typeparam>
    /// <param name="builder">The host application builder.</param>
    /// <param name="connectionName">
    /// The service key of the registered <see cref="TableServiceClient"/>, also used as the service key of the store.
    /// </param>
    /// <returns>The <paramref name="builder"/>, for chaining.</returns>
    public static IHostApplicationBuilder AddAzureTableCheckpointStoreAsDefault<TPosition>(this IHostApplicationBuilder builder, string connectionName)
        where TPosition : IComparable<TPosition>, ISpanFormattable, IParsable<TPosition>
    {
        builder.AddAzureTableCheckpointStore<TPosition>(connectionName);
        builder.Services.AddSingleton(provider => provider.GetRequiredKeyedService<ICheckpointStore<TPosition>>(connectionName));
        return builder;
    }
}
