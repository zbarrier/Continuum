using System.ComponentModel.DataAnnotations;

using Continuum;
using Continuum.EventSourcing.Orleans.CosmosDB;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.Configuration;

/// <summary>
///     Options for the Cosmos DB log consistent storage provider.
/// </summary>
public class CosmosDBLogConsistentStorageOptions
{
    /// <summary>
    ///     Stage of silo lifecycle where storage should be initialized.  Storage must be initialized prior to use.
    /// </summary>
    public int InitStage { get; set; } = ServiceLifecycleStage.ApplicationServices;

    /// <summary>
    ///     The name of the Cosmos DB connection used to resolve the keyed CosmosClient, serializer and type mapper.
    ///     Must be set to a valid Cosmos DB connection name.
    /// </summary>
    [Redact]
    [Required]
    public string ConnectionName { get; set; } = default!;

    /// <summary>
    ///     The Cosmos DB database holding the event container.
    /// </summary>
    [Redact]
    [Required]
    public string DatabaseName { get; set; } = default!;

    /// <summary>
    ///     The Cosmos DB container events are written to. Its partition key path must be <c>/streamName</c>.
    /// </summary>
    [Redact]
    [Required]
    public string ContainerName { get; set; } = default!;

    /// <summary>
    ///     The serializer used to serialize event payloads. It must produce JSON.
    ///     Gets set based on the connection name.
    /// </summary>
    public IGrainStorageSerializer GrainStorageSerializer { get; set; } = default!;

    /// <summary>
    ///     Maximum operations per transactional batch (2-100). The stream header is written in the same batch,
    ///     so at most <c>BatchSize - 1</c> events can be appended per write.
    /// </summary>
    [Required, Range(2, 100)]
    public int BatchSize { get; set; } = 100;

    /// <summary>
    ///     Maximum number of items returned per query page.
    /// </summary>
    [Required, Range(1, 4092)]
    public int QueryMaxItemCount { get; set; } = 1000;

    /// <summary>
    ///     When true, events whose type can no longer be resolved are skipped instead of failing the read.
    /// </summary>
    public bool IgnoreMissingTypes { get; set; } = false;

    /// <summary>
    ///     Used to generate the stream name for a grain.
    ///     The first string is the ServiceId.
    ///     The second string is the ProviderName.
    ///     The third parameter is the GrainId which is 'GrainType/Key'.
    /// </summary>
    [Redact]
    public Func<string, string, GrainId, string> StreamNameFormatter { get; set; } =
        (serviceId, providerName, grainId) => $"{serviceId}/{grainId}";

    /// <summary>
    ///     Generates the id of each appended event item.
    /// </summary>
    [Redact]
    public Func<NewId> EventIdGenerator { get; set; } = NewId.Next;

    /// <summary>
    ///    The type map used to name stored events. Defaults to domain events and metadata.
    /// </summary>
    [Redact]
    public TypeMapKinds TypeMapKinds { get; set; } = TypeMapKinds.DomainEvent | TypeMapKinds.Metadata;

    /// <summary>
    ///     Used to set the stored event DataType to the provided name instead of the CLR 
    ///     type name. Allows us to rename event classes without breaking deserialization.
    ///     Gets set based on the connection name and type map kinds.
    /// </summary>
    [Redact]
    public ITypeMapper TypeMapper { get; set; } = default!;

    /// <summary>
    ///     How long silo startup keeps retrying when Cosmos DB is unreachable while validating the container.
    ///     Startup fails if the container still cannot be read when this window elapses.
    /// </summary>
    public TimeSpan StartupConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Storage operations that consume more request units (RUs) than this threshold are logged as warnings.
    ///     Operations at or below the threshold are logged at debug level.
    /// </summary>
    public double RequestChargeWarningThreshold { get; set; } = 50;
}

/// <summary>
///     Fills in unset <see cref="CosmosDBLogConsistentStorageOptions"/> values from keyed services and defaults.
/// </summary>
public class DefaultCosmosDBLogConsistentStorageOptionsConfigurator : IPostConfigureOptions<CosmosDBLogConsistentStorageOptions>
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Creates the configurator.
    /// </summary>
    /// <param name="serviceProvider">Used to resolve keyed services by connection name.</param>
    public DefaultCosmosDBLogConsistentStorageOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, CosmosDBLogConsistentStorageOptions options)
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
        if (options.StreamNameFormatter is null)
        {
            options.StreamNameFormatter = (serviceId, providerName, grainId) => $"{serviceId}/{grainId}";
        }
        if (options.EventIdGenerator is null)
        {
            options.EventIdGenerator = NewId.Next;
        }
        if (options.TypeMapper is null)
        {
            string key = options.TypeMapKinds.ToTypeMapperKey(options.ConnectionName);
            options.TypeMapper = _serviceProvider.GetRequiredKeyedService<ITypeMapper>(key);
        }
    }
}