using Continuum.Streaming;
using Continuum.Streaming.Orleans.KurrentDB;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Continuum.Orleans.Hosting;

/// <summary>
///     Registers the KurrentDB checkpoint store, for both the self-hosted catch-up subscriptions and the Orleans stream
///     provider.
/// </summary>
public static class KurrentDBCheckpointStoreBuilderExtensions
{
    /// <summary>
    ///     Registers a <see cref="KurrentDBCheckpointStore" /> keyed by a KurrentDB connection name.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <param name="connectionName">The keyed KurrentDB client connection name.</param>
    /// <returns>The host builder.</returns>
    public static IHostApplicationBuilder AddKurrentDBCheckpointStore(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.Services.AddKeyedSingleton<ICheckpointStore<ulong>>(connectionName, (provider, key) =>
        {
            var client = provider.GetRequiredKeyedService<KurrentDBClient>(key);
            return new KurrentDBCheckpointStore(client);
        });

        return builder;
    }
}
