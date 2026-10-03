using System.ComponentModel.DataAnnotations;

using Continuum;
using Continuum.EventSourcing.Orleans.CosmosDB;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Orleans.Runtime;

namespace Orleans.Configuration;

public class CosmosDBLogConsistentStorageOptions
{
    /// <summary>
    ///     Stage of silo lifecycle where storage should be initialized.  Storage must be initialized prior to use.
    /// </summary>
    public int InitStage { get; set; } = ServiceLifecycleStage.ApplicationServices;

    /// <summary>
    ///     The name of the EventStoreDB connection used to get the EventStoreClient. 
    ///     Must be set to a valid EventStoreDB connection name.
    /// </summary>
    [Redact]
    [Required]
    public string ConnectionName { get; set; } = default!;

    [Redact]
    [Required]
    public string DatabaseName { get; set; } = default!;

    [Redact]
    [Required]
    public string ContainerName { get; set; } = default!;

    /// <summary>
    ///     The serializer used to serialize events to a CosmosDB stream. 
    ///     Gets set based on the connection name. Will use the Newtonsoft default Json serializer 
    ///     if no serializer is registered for the connection name.
    /// </summary>
    [Redact]
    [Required]
    public JsonSerializer JsonSerializer { get; set; } = default!;

    public DeleteMode DeleteMode { get; set; } = DeleteMode.Soft;

    [Required, Range(1, 100)]
    public int BatchSize { get; set; } = 100;

    [Required, Range(1, 4092)]
    public int QueryMaxItemCount { get; set; } = 1000;

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

    [Redact]
    public Func<NewId> EventIdGenerator { get; set; } = NewId.Next;

    /// <summary>
    ///    The type of EventData to store in the EventStoreDB. Defaults to domain events.
    /// </summary>
    [Redact]
    public TypeMapKinds TypeMapKinds { get; set; } = TypeMapKinds.DomainEvent | TypeMapKinds.Metadata;

    /// <summary>
    ///     Used to set the EventData.Type property to the provided name instead of the CLR 
    ///     type name. Allows us to rename event classes without breaking deserialization.
    ///     Gets set based on the connection name and type map kinds.
    /// </summary>
    [Redact]
    public ITypeMapper TypeMapper { get; set; } = default!;
}

public class DefaultCosmosDBLogConsistentStorageOptionsConfigurator : IPostConfigureOptions<CosmosDBLogConsistentStorageOptions>
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultCosmosDBLogConsistentStorageOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void PostConfigure(string? name, CosmosDBLogConsistentStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionName))
        {
            // Validator will throw an exception, so we will let it handle it.
            return;
        }
        if (options.JsonSerializer is null)
        {
            options.JsonSerializer = JsonSerializer.CreateDefault();
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