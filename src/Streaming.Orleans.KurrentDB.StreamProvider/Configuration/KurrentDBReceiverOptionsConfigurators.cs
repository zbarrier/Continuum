using Continuum.Streaming;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Orleans.Configuration;

/// <summary>
///     Resolves the checkpoint store for <see cref="KurrentDBCatchupSubscriptionReceiverOptions" /> from the configured
///     checkpoint connection name.
/// </summary>
/// <typeparam name="TOptions">The catch-up receiver options type.</typeparam>
public class DefaultKurrentDBCatchupSubscriptionReceiverOptionsConfigurator<TOptions> : IPostConfigureOptions<TOptions>
    where TOptions : KurrentDBCatchupSubscriptionReceiverOptions
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Creates a new <see cref="DefaultKurrentDBCatchupSubscriptionReceiverOptionsConfigurator{TOptions}" />.
    /// </summary>
    public DefaultKurrentDBCatchupSubscriptionReceiverOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, TOptions options)
    {
        if (options.CheckpointStore is null)
        {
            options.CheckpointStore = string.IsNullOrWhiteSpace(options.CheckpointConnectionName)
                ? _serviceProvider.GetRequiredService<ICheckpointStore<ulong>>()
                : _serviceProvider.GetRequiredKeyedService<ICheckpointStore<ulong>>(options.CheckpointConnectionName);
        }
    }
}
