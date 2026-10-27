using Continuum.Streaming.Orleans.KurrentDB.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;
using Continuum.Streams;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Configuration;

/// <summary>
///     Options for a KurrentDB catch-up subscription.
/// </summary>
public class KurrentDBCatchupSubscriptionOptions
{
    /// <summary>Whether the subscription runs.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>The keyed KurrentDB client connection name to read from.</summary>
    public string ConnectionName { get; set; } = default!;

    /// <summary>The buffering options between reading and processing.</summary>
    public KurrentDBChannelBufferOptions BufferOptions { get; set; } = new();

    /// <summary>The checkpoint commit cadence.</summary>
    public KurrentDBCheckpointMonitorOptions CheckpointMonitorOptions { get; set; } = new();

    /// <summary>The backoff between attempts to process a batch that failed with a transient error.</summary>
    public KurrentDBBatchRetryOptions RetryOptions { get; set; } = new();

    /// <summary>The credentials used to subscribe.</summary>
    public KurrentDBSubscriptionCredentialsOptions Credentials { get; set; } = new();

    /// <summary>The keyed checkpoint store connection name, or empty for the default checkpoint store.</summary>
    public string? CheckpointConnectionName { get; set; } = default!;

    /// <summary>The checkpoint store, resolved from <see cref="CheckpointConnectionName" /> when not set.</summary>
    public ICheckpointStore<ulong> CheckpointStore { get; set; } = default!;

    /// <summary>The serializer for event payloads, resolved from <see cref="ConnectionName" /> when not set.</summary>
    public IGrainStorageSerializer GrainStorageSerializer { get; set; } = default!;

    /// <summary>The kinds of mapped types events are resolved with.</summary>
    public TypeMapKinds TypeMapKinds { get; set; } = TypeMapKinds.DomainEvent;

    /// <summary>The type mapper, resolved from <see cref="TypeMapKinds" /> and <see cref="ConnectionName" /> when not set.</summary>
    public ITypeMapper TypeMapper { get; set; } = default!;

    /// <summary>The stream name parser, resolved from <see cref="ConnectionName" /> when not set, falling back to <see cref="DefaultKurrentDBStreamNameParser" />.</summary>
    public IStreamNameParser StreamNameParser { get; set; } = default!;
}

/// <summary>
///     Buffering options for a catch-up subscription.
/// </summary>
public class KurrentDBChannelBufferOptions
{
    /// <summary>The maximum number of events processed per loop.</summary>
    public int MaxEventsPerProcessLoop { get; set; } = 100;

    /// <summary>The number of buffered events at which reading pauses.</summary>
    public int MaxUnprocessedEventsBeforeStop { get; set; } = 1_000;

    /// <summary>
    ///     How long to wait for the rest of an open transaction when no event arrives before handing it off incomplete.
    ///     A transaction is committed in one go, so a gap this long means its remaining events were filtered out.
    /// </summary>
    public TimeSpan IncompleteTransactionTimeout { get; set; } = TimeSpan.FromSeconds(60);
}

/// <summary>
///     Retry options for a batch that failed with a transient error.
/// </summary>
/// <remarks>
///     Transient failures are retried indefinitely with exponential backoff and jitter, so an outage of a dependency does
///     not require restarting the worker. Non-transient failures stop the subscription.
/// </remarks>
public class KurrentDBBatchRetryOptions
{
    /// <summary>The delay the exponential backoff starts from.</summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>The longest delay between two attempts.</summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);
}



/// <summary>
///     Resolves catch-up subscription dependencies that were not configured explicitly.
/// </summary>
public class DefaultKurrentDBChannelCatchupSubscriptionOptionsConfigurator : IPostConfigureOptions<KurrentDBCatchupSubscriptionOptions>
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultKurrentDBChannelCatchupSubscriptionOptionsConfigurator" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider dependencies are resolved from.</param>
    public DefaultKurrentDBChannelCatchupSubscriptionOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, KurrentDBCatchupSubscriptionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionName))
        {
            // Validator will throw an exception, so we will let it handle it.
            return;
        }
        if (options.CheckpointStore is null)
        {
            options.CheckpointStore = string.IsNullOrWhiteSpace(options.CheckpointConnectionName) 
                ? _serviceProvider.GetRequiredService<ICheckpointStore<ulong>>()
                : _serviceProvider.GetRequiredKeyedService<ICheckpointStore<ulong>>(options.CheckpointConnectionName);
        }
        if (options.GrainStorageSerializer is null)
        {
            options.GrainStorageSerializer = _serviceProvider.GetRequiredKeyedService<IGrainStorageSerializer>(options.ConnectionName);
        }
        if (options.TypeMapper is null)
        {
            string key = options.TypeMapKinds.ToTypeMapperKey(options.ConnectionName);
            options.TypeMapper = _serviceProvider.GetRequiredKeyedService<ITypeMapper>(key);
        }
        if (options.StreamNameParser is null)
        {
            options.StreamNameParser = _serviceProvider.GetKeyedService<IStreamNameParser>(options.ConnectionName) ?? new DefaultKurrentDBStreamNameParser();
        }
    }
}
