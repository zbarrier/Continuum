using System.ComponentModel.DataAnnotations;

using Continuum.Streaming.Orleans.KurrentDB.Monitors;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Continuum.Serialization.Orleans;

using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.Configuration;

/// <summary>
///     Options for a KurrentDB catch-up subscription.
/// </summary>
public class KurrentDBCatchupSubscriptionOptions
{
    /// <summary>Whether the subscription runs.</summary>
    [Required]
    public bool Enabled { get; set; } = false;

    /// <summary>The keyed KurrentDB client connection name to read from.</summary>
    [Required]
    public string ConnectionName { get; set; } = default!;

    /// <summary>The buffering options between reading and processing.</summary>
    [Required]
    public KurrentDBChannelBufferOptions BufferOptions { get; set; } = new();

    /// <summary>The checkpoint commit cadence.</summary>
    [Required]
    public KurrentDBCheckpointMonitorOptions CheckpointMonitorOptions { get; set; } = new();

    /// <summary>The credentials used to subscribe.</summary>
    public KurrentDBSubscriptionCredentialsOptions Credentials { get; set; } = new();

    /// <summary>The keyed checkpoint store connection name, or empty for the default checkpoint store.</summary>
    [Required]
    public string? CheckpointConnectionName { get; set; } = default!;

    /// <summary>The checkpoint store, resolved from <see cref="CheckpointConnectionName" /> when not set.</summary>
    [Required]
    public ICheckpointStore<ulong> CheckpointStore { get; set; } = default!;

    /// <summary>The serializer for event payloads, resolved from <see cref="ConnectionName" /> when not set.</summary>
    [Required]
    public IGrainStorageSerializer GrainStorageSerializer { get; set; } = default!;

    /// <summary>The kinds of mapped types events are resolved with.</summary>
    [Required]
    public TypeMapKinds TypeMapKinds { get; set; } = TypeMapKinds.DomainEvent;

    /// <summary>The type mapper, resolved from <see cref="TypeMapKinds" /> and <see cref="ConnectionName" /> when not set.</summary>
    [Required]
    public ITypeMapper TypeMapper { get; set; } = default!;
}

/// <summary>
///     Buffering options for a catch-up subscription.
/// </summary>
public class KurrentDBChannelBufferOptions
{
    /// <summary>The maximum number of events processed per loop.</summary>
    [Required, Range(1, 100_000)]
    public int MaxEventsPerProcessLoop { get; set; } = 100;

    /// <summary>The number of buffered events at which reading pauses.</summary>
    [Required, Range(1, 1_000_000)]
    public int MaxUnprocessedEventsBeforeStop { get; set; } = 1_000;
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
    }
}
