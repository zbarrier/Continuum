using Continuum;
using Continuum.Orleans.KurrentDB;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Storage;

namespace Orleans.Configuration;

/// <summary>
///     KurrentDB persistent storage options.
/// </summary>
public class KurrentDBGrainStorageOptions : IStorageProviderSerializerOptions
{
    /// <summary>
    ///     Whether or not to delete underlying state stream during a clear operation.
    /// </summary>
    public bool DeleteStateOnClear { get; set; } = false;

    /// <summary>
    ///     The maximum number of state events KurrentDB keeps in each grain's state stream, including clear markers.
    ///     Older events are hidden from reads straight away and removed by KurrentDB when it scavenges. Only the
    ///     latest event is read as the grain state, so values above 1 keep a short history for inspection.
    ///     <see langword="null" /> keeps every event. Defaults to 5.
    /// </summary>
    /// <remarks>
    ///     The limit is written to the stream metadata when the provider creates the stream: on the first write for a
    ///     grain and on the first write after the stream was deleted by a clear. Changing this value does not update
    ///     streams that already exist.
    /// </remarks>
    public int? MaxStateEventCount { get; set; } = 5;

    /// <summary>
    ///     Stage of silo lifecycle where storage should be initialized.  Storage must be initialized prior to use.
    /// </summary>
    public int InitStage { get; set; } = ServiceLifecycleStage.ApplicationServices;

    /// <summary>
    ///     The name of the KurrentDB connection used to get the KurrentDBClient. 
    ///     Must be set to a valid KurrentDB connection name.
    /// </summary>
    public string ConnectionName { get; set; } = default!;

    /// <summary>
    ///     The credentials that have permissions to append/read events. 
    ///     UseDefault should be true for Insecure localhost connections.
    /// </summary>
    [Redact]
    public KurrentDBCredentialsOptions Credentials { get; set; } = new();

    /// <summary>
    ///     The serializer used in serialize state to an KurrentDB stream.
    ///     Gets set based on the connection name.
    /// </summary>
    public IGrainStorageSerializer GrainStorageSerializer { get; set; } = default!;

    /// <summary>
    ///     The content type recorded on each state event. When <see langword="null" />, it is inferred from
    ///     <see cref="GrainStorageSerializer" />: <c>application/json</c> for the Continuum and Orleans JSON serializers and
    ///     <c>application/octet-stream</c> otherwise. Set it explicitly when using a custom JSON serializer.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    ///     Used to generate the stream name for a grain.
    ///     The first string is the ServiceId.
    ///     The second string is the ProviderName.
    ///     The third parameter is the GrainId which is 'GrainType/Key'.
    /// </summary>
    /// <remarks>
    ///     The <c>state__</c> discriminator stays in front of the grain type so a grain's state stream and its
    ///     event stream cannot collide now that the ServiceId no longer separates them. It sits before the first
    ///     <c>-</c>, so it forms part of the category and state streams group separately from event streams.
    /// </remarks>
    public Func<string, string, GrainId, string> StreamNameFormatter { get; set; } =
        (serviceId, providerName, grainId) => $"state__{grainId.Type}-{grainId.Key}";

    /// <summary>
    ///     Generates the ID of each state event appended to KurrentDB. Defaults to sequential GUIDs.
    /// </summary>
    public Func<Guid> EventIdGenerator { get; set; } = NewId.NextSequentialGuid;

    /// <summary>
    ///    The type of EventData to store in the KurrentDB. 
    ///    Defaults to state since this is about grain state persistence.
    /// </summary>
    public TypeMapKinds TypeMapKinds { get; set; } = TypeMapKinds.Snapshot;

    /// <summary>
    ///     Used to set the EventData.Type property to the provided name instead of the CLR 
    ///     type name. Allows us to rename state classes without breaking deserialization.
    ///     Gets set based on the connection name and type map kinds.
    /// </summary>
    public ITypeMapper TypeMapper { get; set; } = default!;
}

/// <summary>
///     Fills in options that depend on the connection name, such as the serializer and type mapper.
/// </summary>
public class DefaultKurrentDBGrainStorageOptionsConfigurator : IPostConfigureOptions<KurrentDBGrainStorageOptions>
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Creates a new instance of the <see cref="DefaultKurrentDBGrainStorageOptionsConfigurator"/> type.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve keyed services.</param>
    public DefaultKurrentDBGrainStorageOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, KurrentDBGrainStorageOptions options)
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
            options.StreamNameFormatter = (serviceId, providerName, grainId) => $"state__{grainId.Type}-{grainId.Key}";
        }
        if (options.EventIdGenerator is null)
        {
            options.EventIdGenerator = NewId.NextSequentialGuid;
        }
        if (options.TypeMapper is null)
        {
            string key = options.TypeMapKinds.ToTypeMapperKey(options.ConnectionName);
            options.TypeMapper = _serviceProvider.GetRequiredKeyedService<ITypeMapper>(key);
        }
    }
}