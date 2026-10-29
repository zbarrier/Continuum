using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Orleans.Configuration;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;

/// <summary>
///     Creates receivers backed by a client checkpointed KurrentDB <c>$all</c> subscription.
/// </summary>
/// <remarks>
///     The read position is owned by the client, so this strategy is validated to a single queue and resolves the
///     shared keyed <see cref="KurrentDBClient" /> for the configured connection.
/// </remarks>
public sealed class KurrentDBCatchUpSubscriptionReceiverFactory : IKurrentDBReceiverFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KurrentDBCatchUpSubscriptionReceiverOptions _receiverOptions;
    private readonly string _connectionName;
    private readonly KurrentDBCheckpointMarkerData _checkpointMarker;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBCatchUpSubscriptionReceiverFactory" />.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the keyed client.</param>
    /// <param name="name">The stream provider name.</param>
    public KurrentDBCatchUpSubscriptionReceiverFactory(IServiceProvider serviceProvider, string name)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        _serviceProvider = serviceProvider;
        _receiverOptions = serviceProvider.GetOptionsByName<KurrentDBCatchUpSubscriptionReceiverOptions>(name);
        _connectionName = serviceProvider.GetOptionsByName<KurrentDBOptions>(name).ConnectionName;
        _checkpointMarker = KurrentDBCheckpointMarkerData.Create(serviceProvider.GetOptionsByName<KurrentDBDataAdapterOptions>(name));
    }

    /// <inheritdoc />
    public KurrentDBReceiverOptionsBase ReceiverOptions => _receiverOptions;

    /// <inheritdoc />
    public bool ClientCheckpointed => true;

    /// <inheritdoc />
    public IKurrentDBReceiver Create(KurrentDBReceiverSettings settings, ILogger logger)
    {
        // The client is a keyed singleton shared by every queue of this connection. It is owned by the container, so
        // receivers must not dispose it.
        var client = _serviceProvider.GetRequiredKeyedService<KurrentDBClient>(_connectionName);
        return new KurrentDBCatchUpSubscriptionReceiver(client, settings, logger, _checkpointMarker);
    }

    /// <inheritdoc />
    public IKurrentDBQueueAdapterReceiver CreateQueueAdapterReceiver(KurrentDBQueueAdapterReceiverContext context) => new KurrentDBQueueAdapterReceiver(context);
}
