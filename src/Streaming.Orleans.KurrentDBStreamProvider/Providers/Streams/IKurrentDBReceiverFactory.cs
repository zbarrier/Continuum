using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Orleans.Configuration;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Creates the <see cref="IKurrentDBReceiver" /> for the subscription strategy selected on a stream provider,
///     and exposes the receiver options belonging to that strategy.
/// </summary>
/// <remarks>
///     One implementation is registered as a keyed service per stream provider name by the
///     <c>UsePersistentSubscriptions</c>/<c>UseAllStreamSubscription</c> configurator extensions. This keeps the
///     strategy selection out of the queue adapter factory and ensures only the selected strategy's options are
///     resolved and validated.
/// </remarks>
public interface IKurrentDBReceiverFactory
{
    /// <summary>
    ///     The receiver options for the selected strategy.
    /// </summary>
    KurrentDBReceiverOptionsBase ReceiverOptions { get; }

    /// <summary>
    ///     Creates a receiver for a single queue.
    /// </summary>
    /// <param name="settings">The per-queue receiver settings.</param>
    /// <param name="position">The last checkpointed position, if any.</param>
    /// <param name="logger">The logger.</param>
    IKurrentDBReceiver Create(KurrentDBReceiverSettings settings, string position, ILogger logger);
}

/// <summary>
///     Creates receivers backed by KurrentDB persistent subscriptions (consumer groups).
/// </summary>
public sealed class KurrentDBPersistentSubscriptionReceiverFactory : IKurrentDBReceiverFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KurrentDBPersistentSubscriptionReceiverOptions _receiverOptions;
    private readonly string _connectionName;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBPersistentSubscriptionReceiverFactory" />.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the keyed subscriptions client.</param>
    /// <param name="name">The stream provider name.</param>
    public KurrentDBPersistentSubscriptionReceiverFactory(IServiceProvider serviceProvider, string name)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        _serviceProvider = serviceProvider;
        _receiverOptions = serviceProvider.GetOptionsByName<KurrentDBPersistentSubscriptionReceiverOptions>(name);
        _connectionName = serviceProvider.GetOptionsByName<KurrentDBOptions>(name).ConnectionName;
    }

    /// <inheritdoc />
    public KurrentDBReceiverOptionsBase ReceiverOptions => _receiverOptions;

    /// <inheritdoc />
    public IKurrentDBReceiver Create(KurrentDBReceiverSettings settings, string position, ILogger logger)
    {
        // The subscriptions client is a keyed singleton shared by every queue of this connection. It is owned by the
        // container, so receivers must not dispose it.
        var client = _serviceProvider.GetRequiredKeyedService<KurrentDBPersistentSubscriptionsClient>(_connectionName);
        return new KurrentDBPersistentSubscriptionReceiver(client, settings, position, logger);
    }
}
