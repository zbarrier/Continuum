using System.ComponentModel.DataAnnotations;

using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Storage;
using Continuum.Streams;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.Configuration;

/// <summary>Where a new subscription starts reading the change feed.</summary>
public enum StartingPositionType
{
    /// <summary>From the beginning of the change feed.</summary>
    BeginningOfTime,
    /// <summary>From the configured starting date and time.</summary>
    FromDateTime,
    /// <summary>From changes made after the subscription starts.</summary>
    Latest
}

/// <summary>Options for a Cosmos DB catch-up subscription.</summary>
public class CosmosDBCatchupSubscriptionOptions
{
    /// <summary>Whether the subscription runs.</summary>
    [Required]
    public bool Enabled { get; set; } = false;

    /// <summary>The keyed CosmosClient connection name.</summary>
    [Required]
    public string ConnectionName { get; set; } = default!;

    /// <summary>The database containing the monitored and lease containers.</summary>
    [Required]
    public string DatabaseName { get; set; } = default!;

    /// <summary>The container holding change feed leases.</summary>
    [Required]
    public string LeaseContainerName { get; set; } = default!;

    /// <summary>The container whose change feed is read.</summary>
    [Required]
    public string MonitoredContainerName { get; set; } = default!;

    /// <summary>The change feed processor instance name.</summary>
    [Required]
    public string InstanceName { get; set; } = default!;

    /// <summary>Where a new subscription starts reading.</summary>
    [Required]
    public StartingPositionType StartingPositionType { get; set; } = StartingPositionType.BeginningOfTime;

    /// <summary>The start time when the starting position type is FromDateTime.</summary>
    public DateTime? StartingPositionDateTime { get; set; } = null;

    /// <summary>The delay between change feed polls.</summary>
    [Required, Range(100, 60000)]
    public int PollingIntervalInMilliseconds { get; set; } = 5000;

    /// <summary>The maximum number of items read per poll.</summary>
    [Required, Range(1, 10000)]
    public int MaxItemsPerPoll { get; set; } = 500;

    /// <summary>Whether stream header items are skipped.</summary>
    [Required]
    public bool IgnoreHeaderEvents { get; set; } = true;

    /// <summary>Deserializes event payloads; resolved by connection name when not set.</summary>
    [Required]
    public IGrainStorageSerializer GrainStorageSerializer { get; set; } = default!;

    /// <summary>Parses stream names; resolved by connection name or defaulted when not set.</summary>
    [Required]
    public IStreamNameParser StreamNameParser { get; set; } = default!;

    /// <summary>The kinds of mapped types used to resolve event types.</summary>
    [Required]
    public TypeMapKinds TypeMapKinds { get; set; } = TypeMapKinds.DomainEvent | TypeMapKinds.Metadata;

    /// <summary>Resolves event types; resolved by connection name and type map kinds when not set.</summary>
    [Required]
    public ITypeMapper TypeMapper { get; set; } = default!;
}

/// <summary>Resolves the service-backed options that were not set in code.</summary>
public class DefaultCosmosDBCatchupSubscriptionOptionsConfigurator : IPostConfigureOptions<CosmosDBCatchupSubscriptionOptions>
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Creates the configurator.</summary>
    public DefaultCosmosDBCatchupSubscriptionOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public void PostConfigure(string? name, CosmosDBCatchupSubscriptionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionName))
        {
            // Validator will throw an exception, so we will let it handle it.
            return;
        }
        if (options.GrainStorageSerializer is null)
        {
            options.GrainStorageSerializer = _serviceProvider.GetRequiredKeyedService<IGrainStorageSerializer>(options.ConnectionName);
        }
        if (options.StreamNameParser is null)
        {
            options.StreamNameParser = _serviceProvider.GetKeyedService<IStreamNameParser>(options.ConnectionName) ?? new DefaultCosmosDBStreamNameParser();
        }
        if (options.TypeMapper is null)
        {
            string key = options.TypeMapKinds.ToTypeMapperKey(options.ConnectionName);
            options.TypeMapper = _serviceProvider.GetRequiredKeyedService<ITypeMapper>(key);
        }
    }
}