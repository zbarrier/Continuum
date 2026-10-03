using Continuum;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Storage;

namespace Orleans.Configuration;

public sealed class KurrentDBLogConsistentStorageOptions : IStorageProviderSerializerOptions
{
    /// <summary>
    ///     Stage of silo lifecycle where storage should be initialized.  Storage must be initialized prior to use.
    /// </summary>
    public int InitStage { get; set; } = ServiceLifecycleStage.ApplicationServices;

    /// <summary>
    ///     The name of the KurrentDB connection used to get the KurrentDBClient. 
    ///     Must be set to a valid Kurrent connection name.
    /// </summary>
    [Redact]
    public string ConnectionName { get; set; } = default!;

    /// <summary>
    ///     The credentials that have permissions to append/read events. 
    ///     UseDefault should be true for Insecure localhost connections.
    /// </summary>
    [Redact]
    public KurrentDBLogConsistentStorageCredentialsOptions Credentials { get; set; } = new();

    /// <summary>
    ///     The serializer used in serialize events to an Kurrent stream. 
    ///     Gets set based on the connection name.
    /// </summary>
    public IGrainStorageSerializer GrainStorageSerializer { get; set; } = default!;

    /// <summary>
    ///     Used to generate the stream name for a grain.
    ///     The first string is the ServiceId.
    ///     The second string is the ProviderName.
    ///     The third parameter is the GrainId which is 'GrainType/Key'.
    /// </summary>
    /// <remarks>
    ///     The name is <c>{GrainType}-{GrainKey}</c> so that the grain type is the stream category. KurrentDB
    ///     derives its <c>$ce-</c> category projections from the text before the first <c>-</c>, so prefixing the
    ///     name would fold the prefix into the category and make every subscription filter carry it. The ServiceId
    ///     is deliberately not included: it scoped streams to a cluster, but it also meant no two parts of the
    ///     system could agree on a category without agreeing on the ServiceId first. A deployment that shares one
    ///     KurrentDB instance between services must supply its own formatter to keep them apart.
    /// </remarks>
    [Redact]
    public Func<string, string, GrainId, string> StreamNameFormatter { get; set; } =
        (serviceId, providerName, grainId) => $"{grainId.Type}-{grainId.Key}";

    [Redact]
    public Func<Guid> EventIdGenerator { get; set; } = NewId.NextSequentialGuid;

    /// <summary>
    ///    The type of EventData to store in KurrentDB. Defaults to domain events.
    /// </summary>
    [Redact]
    public TypeMapKinds TypeMapKinds { get; set; } = TypeMapKinds.DomainEvent;

    /// <summary>
    ///     Used to set the EventData.Type property to the provided name instead of the CLR 
    ///     type name. Allows us to rename event classes without breaking deserialization.
    ///     Gets set based on the connection name and type map kinds.
    /// </summary>
    [Redact]
    public ITypeMapper TypeMapper { get; set; } = default!;
}

public sealed class KurrentDBLogConsistentStorageCredentialsOptions
{
    public bool UseDefault { get; set; } = true;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string AuthToken { get; set; } = default!;

    public UserCredentials? ToUserCredentials()
    {
        if (UseDefault)
        {
            return null;
        }
        return !string.IsNullOrWhiteSpace(AuthToken)
            ? new UserCredentials(AuthToken)
            : new UserCredentials(Username, Password);
    }
}

public class DefaultKurrentDBLogConsistentStorageOptionsConfigurator : IPostConfigureOptions<KurrentDBLogConsistentStorageOptions>
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultKurrentDBLogConsistentStorageOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void PostConfigure(string? name, KurrentDBLogConsistentStorageOptions options)
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
            options.StreamNameFormatter = (serviceId, providerName, grainId) => $"{grainId.Type}-{grainId.Key}";
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
