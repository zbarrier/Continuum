using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Persistent;

/// <summary>
///     Creates receivers backed by KurrentDB persistent subscriptions (consumer groups).
/// </summary>
public sealed class KurrentDBPersistentSubscriptionReceiverFactory : IKurrentDBReceiverFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KurrentDBPersistentSubscriptionReceiverOptions _receiverOptions;
    private readonly KurrentDBOptions _options;

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
        _options = serviceProvider.GetOptionsByName<KurrentDBOptions>(name);
    }

    /// <inheritdoc />
    public KurrentDBReceiverOptionsBase ReceiverOptions => _receiverOptions;

    /// <inheritdoc />
    public bool ClientCheckpointed => false;

    /// <inheritdoc />
    public StreamProviderDirection Direction => StreamProviderDirection.ReadWrite;

    /// <inheritdoc />
    public IKurrentDBReceiver Create(KurrentDBReceiverSettings settings, ILogger logger)
    {
        // The subscriptions client is a keyed singleton shared by every queue of this connection. It is owned by the
        // container, so receivers must not dispose it.
        var client = _serviceProvider.GetRequiredKeyedService<KurrentDBPersistentSubscriptionsClient>(_options.ConnectionName);
        return new KurrentDBPersistentSubscriptionReceiver(client, settings, logger);
    }

    /// <inheritdoc />
    public IKurrentDBQueueAdapterReceiver CreateQueueAdapterReceiver(KurrentDBQueueAdapterReceiverContext context) => new KurrentDBPersistentQueueAdapterReceiver(context);

    /// <inheritdoc />
    public IKurrentDBProducer CreateProducer(string queueName, ILoggerFactory loggerFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName, nameof(queueName));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
        var producerSettings = new KurrentProducerSettings
                               {
                                   Options = _options,
                                   QueueName = queueName
                               };
        // The client is a keyed singleton shared by every queue of this connection.
        var client = _serviceProvider.GetRequiredKeyedService<KurrentDBClient>(_options.ConnectionName);
        var producer = new KurrentDBProducer(client, producerSettings, loggerFactory.CreateLogger<KurrentDBProducer>());
        producer.Init();
        return producer;
    }
}
