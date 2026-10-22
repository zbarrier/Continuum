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
///     Validates <see cref="KurrentDBAllStreamReceiverOptions" /> for a named stream provider.
/// </summary>
public class KurrentDBAllStreamReceiverOptionsValidator : IConfigurationValidator
{
    private readonly KurrentDBAllStreamReceiverOptions _options;
    private readonly KurrentDBOptions _kurrentDBOptions;
    private readonly string _name;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBAllStreamReceiverOptionsValidator" />.
    /// </summary>
    public KurrentDBAllStreamReceiverOptionsValidator(KurrentDBAllStreamReceiverOptions options, KurrentDBOptions kurrentDBOptions, string name)
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
            throw new OrleansConfigurationException($"{nameof(KurrentDBAllStreamReceiverOptions)} on stream provider {_name} is invalid. An all-stream subscription requires exactly one entry in {nameof(KurrentDBOptions)}.{nameof(KurrentDBOptions.Queues)}, but {_kurrentDBOptions.Queues?.Count ?? 0} were configured.");
        }
        if (_options.CheckpointStore is null)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBAllStreamReceiverOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBAllStreamReceiverOptions.CheckpointStore)} could not be resolved. Register an ICheckpointStore<ulong> for the configured {nameof(KurrentDBAllStreamReceiverOptions.CheckpointConnectionName)} or set it explicitly.");
        }
        if (_options.CheckpointMonitorOptions is null)
        {
                        throw new OrleansConfigurationException($"{nameof(KurrentDBAllStreamReceiverOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBAllStreamReceiverOptions.CheckpointMonitorOptions)} is required.");
                }
            }
}
