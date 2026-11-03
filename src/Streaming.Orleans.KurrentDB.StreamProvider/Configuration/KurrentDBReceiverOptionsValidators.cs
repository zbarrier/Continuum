using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;
using Orleans.Runtime;

namespace Orleans.Configuration;

/// <summary>
///     Validates <see cref="KurrentDBPersistentSubscriptionReceiverOptions" /> for a named stream provider.
/// </summary>
public class KurrentDBPersistentSubscriptionReceiverOptionsValidator : IConfigurationValidator
{
    private readonly KurrentDBPersistentSubscriptionReceiverOptions _options;
    private readonly string _name;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBPersistentSubscriptionReceiverOptionsValidator" />.
    /// </summary>
    public KurrentDBPersistentSubscriptionReceiverOptionsValidator(KurrentDBPersistentSubscriptionReceiverOptions options, string name)
    {
        _options = options;
        _name = name;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (_options.SubscriptionSettings is null)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBPersistentSubscriptionReceiverOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBPersistentSubscriptionReceiverOptions.SubscriptionSettings)} is required.");
        }
        if (_options.PrefetchCount <= 0)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBPersistentSubscriptionReceiverOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBPersistentSubscriptionReceiverOptions.PrefetchCount)} must be greater than zero.");
        }
            }
        }

/// <summary>
///     Validates <see cref="KurrentDBCatchUpSubscriptionReceiverOptions" /> for a named stream provider.
/// </summary>
public class KurrentDBCatchUpSubscriptionReceiverOptionsValidator : IConfigurationValidator
{
    private readonly KurrentDBCatchUpSubscriptionReceiverOptions _options;
    private readonly KurrentDBOptions _kurrentDBOptions;
    private readonly string _name;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBCatchUpSubscriptionReceiverOptionsValidator" />.
    /// </summary>
    public KurrentDBCatchUpSubscriptionReceiverOptionsValidator(KurrentDBCatchUpSubscriptionReceiverOptions options, KurrentDBOptions kurrentDBOptions, string name)
    {
        _options = options;
        _kurrentDBOptions = kurrentDBOptions;
        _name = name;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        // A $all subscription is global. One receiver is created per queue, so more than one queue would deliver
        // every event once per queue and have every receiver contend over the same checkpoint stream.
        if (_kurrentDBOptions.Queues is null || _kurrentDBOptions.Queues.Count != 1)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBCatchUpSubscriptionReceiverOptions)} on stream provider {_name} is invalid. A catch-up subscription requires exactly one entry in {nameof(KurrentDBOptions)}.{nameof(KurrentDBOptions.Queues)}, but {_kurrentDBOptions.Queues?.Count ?? 0} were configured.");
        }
        if (_options.CheckpointStore is null)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBCatchUpSubscriptionReceiverOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBCatchUpSubscriptionReceiverOptions.CheckpointStore)} could not be resolved. Register an ICheckpointStore<ulong> for the configured {nameof(KurrentDBCatchUpSubscriptionReceiverOptions.CheckpointConnectionName)} or set it explicitly.");
        }
        if (_options.CheckpointMonitorOptions is null)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBCatchUpSubscriptionReceiverOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBCatchUpSubscriptionReceiverOptions.CheckpointMonitorOptions)} is required.");
        }
        if (_options.BufferCapacityMultiplier < 1 || _options.BufferCapacityMultiplier > KurrentDBCatchUpSubscriptionReceiver.MaxBufferCapacityMultiplier)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBCatchUpSubscriptionReceiverOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBCatchUpSubscriptionReceiverOptions.BufferCapacityMultiplier)} must be between 1 and {KurrentDBCatchUpSubscriptionReceiver.MaxBufferCapacityMultiplier}, but was {_options.BufferCapacityMultiplier}.");
        }
    }
}
