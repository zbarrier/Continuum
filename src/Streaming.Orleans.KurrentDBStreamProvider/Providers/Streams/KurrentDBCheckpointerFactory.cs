using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Orleans.Serialization;
using Orleans.Streams;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Factory for creating <see cref="KurrentDBCheckpointer" /> instances.
/// </summary>
public class KurrentDBCheckpointerFactory : IStreamQueueCheckpointerFactory
{
    private readonly string _providerName;
    private readonly KurrentDBStreamCheckpointerOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly Serializer _serializer;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ClusterOptions _clusterOptions;

    /// <summary>
    ///     Creates an instance of the <see cref="KurrentDBCheckpointerFactory" /> using the provided IServiceProvider and providerName.
    /// </summary>
    /// <param name="serviceProvider">The IServiceProvider used for dependency injection.</param>
    /// <param name="providerName">The name used to retrieve options and services from the IServiceProvider.</param>
    /// <returns>An instance of the <see cref="KurrentDBCheckpointerFactory" />.</returns>
    public static KurrentDBCheckpointerFactory CreateFactory(IServiceProvider serviceProvider, string providerName)
    {
        var options = serviceProvider.GetOptionsByName<KurrentDBStreamCheckpointerOptions>(providerName);
        var clusterOptions = serviceProvider.GetProviderClusterOptions(providerName);
        return ActivatorUtilities.CreateInstance<KurrentDBCheckpointerFactory>(serviceProvider, providerName, options, clusterOptions);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBCheckpointerFactory" /> class with the specified parameters.
    /// </summary>
    /// <param name="providerName">The name of the stream provider.</param>
    /// <param name="options">The options for the stream checkpointer.</param>
    /// <param name="clusterOptions">The cluster options.</param>
    /// <param name="serviceProvider">Used to resolve the shared keyed client when a connection name is configured.</param>
    /// <param name="serializer">The serializer used for state manager.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public KurrentDBCheckpointerFactory(string providerName, KurrentDBStreamCheckpointerOptions options, IOptions<ClusterOptions> clusterOptions, IServiceProvider serviceProvider, Serializer serializer, ILoggerFactory loggerFactory)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName, nameof(providerName));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(clusterOptions, nameof(clusterOptions));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
        _providerName = providerName;
        _options = options;
        _clusterOptions = clusterOptions.Value;
        _serviceProvider = serviceProvider;
        _serializer = serializer;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    ///     Creates a stream checkpointer for the specified queue.
    /// </summary>
    /// <param name="queue">The queue name.</param>
    /// <returns>The stream checkpointer.</returns>
    public Task<IStreamQueueCheckpointer<string>> Create(string queue) => Create(queue, CancellationToken.None);

    /// <summary>
    ///     Creates a stream checkpointer for the specified queue.
    /// </summary>
    /// <param name="queue">The queue name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stream checkpointer.</returns>
    public Task<IStreamQueueCheckpointer<string>> Create(string queue, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IStreamQueueCheckpointer<string>>(KurrentDBCheckpointer.Create(_clusterOptions.ServiceId, _providerName, queue, _options, _serviceProvider, _serializer, _loggerFactory));
    }

}
