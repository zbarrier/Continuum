using Continuum.Streaming;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Orleans.Configuration;

/// <summary>
///     Resolves the checkpoint store for <see cref="KurrentDBAllStreamReceiverOptions" /> from the configured
///     checkpoint connection name.
/// </summary>
public class DefaultKurrentDBAllStreamReceiverOptionsConfigurator : IPostConfigureOptions<KurrentDBAllStreamReceiverOptions>
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Creates a new <see cref="DefaultKurrentDBAllStreamReceiverOptionsConfigurator" />.
    /// </summary>
    public DefaultKurrentDBAllStreamReceiverOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, KurrentDBAllStreamReceiverOptions options)
    {
        if (options.CheckpointStore is null)
        {
            options.CheckpointStore = string.IsNullOrWhiteSpace(options.CheckpointConnectionName)
                ? _serviceProvider.GetRequiredService<ICheckpointStore<ulong>>()
                : _serviceProvider.GetRequiredKeyedService<ICheckpointStore<ulong>>(options.CheckpointConnectionName);
        }
    }
}
