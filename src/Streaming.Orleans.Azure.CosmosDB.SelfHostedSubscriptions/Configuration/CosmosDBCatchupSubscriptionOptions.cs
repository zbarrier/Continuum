using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Storage;
using Continuum.Streams;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Configuration;

/// <summary>Where a new subscription starts reading the change feed.</summary>
public enum CosmosDBStartFrom
{
    /// <summary>From the beginning of the change feed.</summary>
    Beginning,
    /// <summary>From the configured starting date and time.</summary>
    DateTime,
    /// <summary>From changes made after the subscription starts.</summary>
    Now
}

/// <summary>Options for a Cosmos DB catch-up subscription.</summary>
public class CosmosDBCatchupSubscriptionOptions
{
    /// <summary>Whether the subscription runs.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>The keyed CosmosClient connection name.</summary>
    public string ConnectionName { get; set; } = default!;

    /// <summary>The database containing the monitored and lease containers.</summary>
    public string DatabaseName { get; set; } = default!;

    /// <summary>The container holding change feed leases.</summary>
    public string LeaseContainerName { get; set; } = default!;

    /// <summary>The container whose change feed is read.</summary>
    public string MonitoredContainerName { get; set; } = default!;

    /// <summary>The change feed processor instance name.</summary>
    public string InstanceName { get; set; } = default!;

    /// <summary>Where a new subscription starts reading.</summary>
    public CosmosDBStartFrom StartFrom { get; set; } = CosmosDBStartFrom.Beginning;

    /// <summary>The start time when <see cref="StartFrom" /> is <see cref="CosmosDBStartFrom.DateTime" />.</summary>
    public DateTime? StartFromDateTime { get; set; } = null;

    /// <summary>The delay between change feed polls.</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>The maximum number of items read per poll.</summary>
    public int MaxItemsPerPoll { get; set; } = 500;

    /// <summary>
    ///     What the subscription does when it reads an event whose type the type mapper cannot resolve. Defaults to
    ///     <see cref="UnknownEventTypeBehavior.Halt" />, which fails the batch so the change feed processor retries the
    ///     lease without advancing past the event.
    /// </summary>
    public UnknownEventTypeBehavior UnknownEventTypeBehavior { get; set; } = UnknownEventTypeBehavior.Halt;

    /// <summary>Deserializes event payloads; resolved by connection name when not set.</summary>
    public IGrainStorageSerializer GrainStorageSerializer { get; set; } = default!;

    /// <summary>Parses stream names; resolved by connection name or defaulted when not set.</summary>
    public IStreamNameParser StreamNameParser { get; set; } = default!;

    /// <summary>The kinds of mapped types used to resolve event types.</summary>
    public TypeMapKinds TypeMapKinds { get; set; } = TypeMapKinds.DomainEvent | TypeMapKinds.Metadata;

    /// <summary>Resolves event types; resolved by connection name and type map kinds when not set.</summary>
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