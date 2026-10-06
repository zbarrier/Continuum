using Continuum;
using Continuum.Streaming;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Runtime;

namespace Orleans.Configuration;

/// <summary>
///     Options for the KurrentDB stream data adapter, which is the serialization boundary of the stream provider.
/// </summary>
/// <remarks>
///     The data adapter owns both directions of serialization: <c>ToQueueMessage</c> on the producer side and the
///     batch container on the consumer side. The type mapper and serializer therefore belong here rather than on the
///     receiver options, so that events written by this provider are readable by the KurrentDB catch-up subscriptions.
/// </remarks>
public class KurrentDBDataAdapterOptions
{
    /// <summary>
    ///     The name of the KurrentDB connection used to resolve the keyed serializer and type mapper.
    /// </summary>
    [Redact]
    public string ConnectionName { get; set; } = default!;

    /// <summary>
    ///     The kinds of type map entries used when resolving event type names.
    /// </summary>
    /// <remarks>
    ///     This is a <see cref="FlagsAttribute" /> enum, so a combined value is valid. The registration in
    ///     <c>AddDefaultTypeMapAttributeMappers</c> keys a mapper by each individual flag and by the exact combined
    ///     value it was called with, so this must match one of those keys. Defaulting to
    ///     <see cref="TypeMapKinds.DomainEvent" /> alone keeps the lookup working when a provider registers only that
    ///     kind; widen it to match whatever the connection actually registered.
    /// </remarks>
    [Redact]
    public TypeMapKinds TypeMapKinds { get; set; } = TypeMapKinds.DomainEvent;

    /// <summary>
    ///     Used to set the KurrentDB <c>EventType</c> to the mapped name instead of the CLR type name, and to resolve
    ///     the CLR type when reading. Allows event classes to be renamed without breaking deserialization.
    /// </summary>
    [Redact]
    public ITypeMapper TypeMapper { get; set; } = default!;

    /// <summary>
    ///     The serializer used to write and read event payloads.
    /// </summary>
    [Redact]
    public IStreamedEventSerde StreamEventSerde { get; set; } = default!;

    /// <summary>
    ///     The KurrentDB content type recorded on each event this provider appends.
    /// </summary>
    /// <remarks>
    ///     This describes what <see cref="StreamEventSerde" /> produces. The default serde is JSON, which KurrentDB
    ///     can index and project over; change this alongside the serde if a binary format is configured, otherwise the
    ///     server will try to read the payload as JSON.
    /// </remarks>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    ///     What the provider does when an event's type cannot be resolved by <see cref="TypeMapper" />.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="KurrentDBUnknownEventTypeBehavior.Halt" />, because an event that cannot be read is
    ///     usually a missing registration rather than a foreign event, and silently moving past it lets the checkpoint
    ///     advance beyond an event no consumer ever saw.
    /// </remarks>
    public KurrentDBUnknownEventTypeBehavior UnknownEventTypeBehavior { get; set; } = KurrentDBUnknownEventTypeBehavior.Halt;

    /// <summary>
    ///     Maps a KurrentDB stream name to the Orleans <see cref="StreamId" /> consumers subscribe to.
    /// </summary>
    /// <remarks>
    ///     Events written by this provider carry the <see cref="StreamId" /> in the KurrentDB event metadata, but
    ///     events written by the event sourcing storage do not, so for those the identity has to come from the stream
    ///     name itself. The default splits on the first '-', mapping "snack-123" to namespace "snack" and key "123".
    ///     Override this when a different naming convention is used.
    /// </remarks>
    public Func<string, StreamId> StreamIdMapper { get; set; } = DefaultStreamIdMapper;

    /// <summary>
    ///     Maps "category-key" stream names onto a <see cref="StreamId" /> of the same shape.
    /// </summary>
    /// <remarks>
    ///     Only the first '-' is treated as the separator, so keys containing '-' (such as GUIDs) survive intact. A
    ///     stream name without a separator becomes a key with an empty namespace rather than being rejected, since
    ///     failing here would break cache ingest for every stream sharing the queue.
    /// </remarks>
    public static StreamId DefaultStreamIdMapper(string streamName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamName, nameof(streamName));
        var separatorIndex = streamName.IndexOf('-', StringComparison.Ordinal);
        if (separatorIndex <= 0 || separatorIndex == streamName.Length - 1)
        {
            return StreamId.Create(string.Empty, streamName);
        }
        return StreamId.Create(streamName[..separatorIndex], streamName[(separatorIndex + 1)..]);
    }
}

/// <summary>
///     Resolves the serializer and type mapper for <see cref="KurrentDBDataAdapterOptions" /> from the connection name.
/// </summary>
public class DefaultKurrentDBDataAdapterOptionsConfigurator : IPostConfigureOptions<KurrentDBDataAdapterOptions>
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Creates a new <see cref="DefaultKurrentDBDataAdapterOptionsConfigurator" />.
    /// </summary>
    public DefaultKurrentDBDataAdapterOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, KurrentDBDataAdapterOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionName) && !string.IsNullOrWhiteSpace(name))
        {
            // Fall back to the connection name configured on the matching stream provider.
            options.ConnectionName = _serviceProvider.GetOptionsByName<KurrentDBOptions>(name).ConnectionName;
        }
        if (string.IsNullOrWhiteSpace(options.ConnectionName))
        {
            // Validator will throw an exception, so we will let it handle it.
            return;
        }
        // Resolved without throwing so a missing registration surfaces through the validator, which names the
        // stream provider and the key that was looked for, rather than as a bare DI exception during silo start up.
        if (options.StreamEventSerde is null)
        {
            options.StreamEventSerde = _serviceProvider.GetKeyedService<IStreamedEventSerde>(options.ConnectionName)!;
        }
        if (options.TypeMapper is null)
        {
            string key = options.TypeMapKinds.ToTypeMapperKey(options.ConnectionName);
            options.TypeMapper = _serviceProvider.GetKeyedService<ITypeMapper>(key)!;
        }
    }
}

/// <summary>
///     Validates <see cref="KurrentDBDataAdapterOptions" /> for a named stream provider.
/// </summary>
public class KurrentDBDataAdapterOptionsValidator : IConfigurationValidator
{
    private readonly KurrentDBDataAdapterOptions _options;
    private readonly string _name;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBDataAdapterOptionsValidator" />.
    /// </summary>
    public KurrentDBDataAdapterOptionsValidator(KurrentDBDataAdapterOptions options, string name)
    {
        _options = options;
        _name = name;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionName))
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBDataAdapterOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBDataAdapterOptions.ConnectionName)} is required.");
        }
        if (_options.StreamIdMapper is null)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBDataAdapterOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBDataAdapterOptions.StreamIdMapper)} is required.");
        }
        if (_options.StreamEventSerde is null)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBDataAdapterOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBDataAdapterOptions.StreamEventSerde)} could not be resolved. Register a keyed IStreamedEventSerde for \"{_options.ConnectionName}\" or set it explicitly.");
        }
        if (_options.TypeMapper is null)
        {
            throw new OrleansConfigurationException($"{nameof(KurrentDBDataAdapterOptions)} on stream provider {_name} is invalid. {nameof(KurrentDBDataAdapterOptions.TypeMapper)} could not be resolved. Register a keyed ITypeMapper for \"{_options.TypeMapKinds.ToTypeMapperKey(_options.ConnectionName)}\" or set it explicitly.");
        }
    }
}
