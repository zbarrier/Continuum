using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;
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
///     Validates the options shared by the catch-up subscription strategies for a named stream provider.
/// </summary>
public abstract class KurrentDBCatchupSubscriptionReceiverOptionsValidator : IConfigurationValidator
{
    private readonly KurrentDBCatchupSubscriptionReceiverOptions _options;
    private readonly KurrentDBOptions _kurrentDBOptions;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBCatchupSubscriptionReceiverOptionsValidator" />.
    /// </summary>
    private protected KurrentDBCatchupSubscriptionReceiverOptionsValidator(KurrentDBCatchupSubscriptionReceiverOptions options, KurrentDBOptions kurrentDBOptions, string name)
    {
        _options = options;
        _kurrentDBOptions = kurrentDBOptions;
        Name = name;
    }

    /// <summary>
    ///     The stream provider name.
    /// </summary>
    protected string Name { get; }

    /// <summary>
    ///     The name of the options type being validated, used in error messages.
    /// </summary>
    protected abstract string OptionsName { get; }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        // A catch-up subscription reads one source. One receiver is created per queue, so more than one queue would
        // subscribe more than once and have every receiver contend over the same checkpoint.
        if (_kurrentDBOptions.Queues is null || _kurrentDBOptions.Queues.Count != 1)
        {
            throw new OrleansConfigurationException($"{OptionsName} on stream provider {Name} is invalid. A catch-up subscription requires exactly one entry in {nameof(KurrentDBOptions)}.{nameof(KurrentDBOptions.Queues)}, but {_kurrentDBOptions.Queues?.Count ?? 0} were configured.");
        }
        ValidateSource(_kurrentDBOptions.Queues[0]);
        if (_options.CheckpointStore is null)
        {
            throw new OrleansConfigurationException($"{OptionsName} on stream provider {Name} is invalid. {nameof(KurrentDBCatchupSubscriptionReceiverOptions.CheckpointStore)} could not be resolved. Register an ICheckpointStore<ulong> for the configured {nameof(KurrentDBCatchupSubscriptionReceiverOptions.CheckpointConnectionName)} or set it explicitly.");
        }
        if (_options.CheckpointMonitorOptions is null)
        {
            throw new OrleansConfigurationException($"{OptionsName} on stream provider {Name} is invalid. {nameof(KurrentDBCatchupSubscriptionReceiverOptions.CheckpointMonitorOptions)} is required.");
        }
        if (_options.BufferCapacityMultiplier < 1 || _options.BufferCapacityMultiplier > KurrentDBCatchupSubscriptionReceiver.MaxBufferCapacityMultiplier)
        {
            throw new OrleansConfigurationException($"{OptionsName} on stream provider {Name} is invalid. {nameof(KurrentDBCatchupSubscriptionReceiverOptions.BufferCapacityMultiplier)} must be between 1 and {KurrentDBCatchupSubscriptionReceiver.MaxBufferCapacityMultiplier}, but was {_options.BufferCapacityMultiplier}.");
        }
        if (_options.IncompleteTransactionTimeout <= TimeSpan.Zero)
        {
            throw new OrleansConfigurationException($"{OptionsName} on stream provider {Name} is invalid. {nameof(KurrentDBCatchupSubscriptionReceiverOptions.IncompleteTransactionTimeout)} must be greater than zero, but was {_options.IncompleteTransactionTimeout}.");
        }
    }

    /// <summary>
    ///     Validates the single configured queue, which names the subscribed source.
    /// </summary>
    /// <param name="queue">The configured queue.</param>
    protected abstract void ValidateSource(string queue);
}

/// <summary>
///     Validates <see cref="KurrentDBAllStreamCatchupSubscriptionReceiverOptions" /> for a named stream provider.
/// </summary>
public sealed class KurrentDBAllStreamCatchupSubscriptionReceiverOptionsValidator : KurrentDBCatchupSubscriptionReceiverOptionsValidator
{
    /// <summary>
    ///     Creates a new <see cref="KurrentDBAllStreamCatchupSubscriptionReceiverOptionsValidator" />.
    /// </summary>
    public KurrentDBAllStreamCatchupSubscriptionReceiverOptionsValidator(KurrentDBAllStreamCatchupSubscriptionReceiverOptions options, KurrentDBOptions kurrentDBOptions, string name)
        : base(options, kurrentDBOptions, name)
    {
    }

    /// <inheritdoc />
    protected override string OptionsName => nameof(KurrentDBAllStreamCatchupSubscriptionReceiverOptions);

    /// <inheritdoc />
    protected override void ValidateSource(string queue)
    {
        if (!string.Equals(queue, KurrentDBStreams.All, StringComparison.Ordinal))
        {
            throw new OrleansConfigurationException($"{OptionsName} on stream provider {Name} is invalid. A $all catch-up subscription requires the queue to be '{KurrentDBStreams.All}', but was '{queue}'. Use a single stream catch-up subscription to subscribe to another stream.");
        }
    }
}

/// <summary>
///     Validates <see cref="KurrentDBStreamCatchupSubscriptionReceiverOptions" /> for a named stream provider.
/// </summary>
public sealed class KurrentDBStreamCatchupSubscriptionReceiverOptionsValidator : KurrentDBCatchupSubscriptionReceiverOptionsValidator
{
    /// <summary>
    ///     Creates a new <see cref="KurrentDBStreamCatchupSubscriptionReceiverOptionsValidator" />.
    /// </summary>
    public KurrentDBStreamCatchupSubscriptionReceiverOptionsValidator(KurrentDBStreamCatchupSubscriptionReceiverOptions options, KurrentDBOptions kurrentDBOptions, string name)
        : base(options, kurrentDBOptions, name)
    {
    }

    /// <inheritdoc />
    protected override string OptionsName => nameof(KurrentDBStreamCatchupSubscriptionReceiverOptions);

    /// <inheritdoc />
    protected override void ValidateSource(string queue)
    {
        if (string.IsNullOrWhiteSpace(queue))
        {
            throw new OrleansConfigurationException($"{OptionsName} on stream provider {Name} is invalid. A single stream catch-up subscription requires the queue to name the stream to subscribe to.");
        }
        if (KurrentDBStreams.IsAll(queue))
        {
            throw new OrleansConfigurationException($"{OptionsName} on stream provider {Name} is invalid. A single stream catch-up subscription cannot subscribe to '{KurrentDBStreams.All}'. Use a $all catch-up subscription instead.");
        }
    }
}
