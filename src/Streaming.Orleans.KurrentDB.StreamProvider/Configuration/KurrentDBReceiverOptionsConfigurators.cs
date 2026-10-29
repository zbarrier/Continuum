using Continuum.Streaming;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Orleans.Configuration;

/// <summary>
///     Resolves the checkpoint store for <see cref="KurrentDBCatchUpSubscriptionReceiverOptions" /> from the configured
///     checkpoint connection name.
/// </summary>
public class DefaultKurrentDBCatchUpSubscriptionReceiverOptionsConfigurator : IPostConfigureOptions<KurrentDBCatchUpSubscriptionReceiverOptions>
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Creates a new <see cref="DefaultKurrentDBCatchUpSubscriptionReceiverOptionsConfigurator" />.
    /// </summary>
    public DefaultKurrentDBCatchUpSubscriptionReceiverOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, KurrentDBCatchUpSubscriptionReceiverOptions options)
    {
        if (options.CheckpointStore is null)
        {
            options.CheckpointStore = string.IsNullOrWhiteSpace(options.CheckpointConnectionName)
                ? _serviceProvider.GetRequiredService<ICheckpointStore<ulong>>()
                : _serviceProvider.GetRequiredKeyedService<ICheckpointStore<ulong>>(options.CheckpointConnectionName);
        }
    }
}
