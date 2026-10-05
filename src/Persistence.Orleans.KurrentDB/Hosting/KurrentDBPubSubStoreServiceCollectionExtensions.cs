using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Serialization;
using Orleans.Storage;

namespace Orleans.Hosting;

/// <summary>
///     Registers KurrentDB as the backing store for Orleans streaming pub/sub subscription state.
/// </summary>
/// <remarks>
///     <para>
///         Orleans persists streaming pub/sub subscription state through a grain storage provider named
///         <c>PubSubStore</c>. The state type is <c>Orleans.Streams.PubSubGrainState</c>, which differs from
///         application state in two ways that the ordinary
///         <see cref="KurrentDBGrainStorageServiceCollectionExtensions.AddKurrentDBGrainStorage(IServiceCollection, string, Action{KurrentDBGrainStorageOptions})" />
///         registration does not account for:
///     </para>
///     <list type="number">
///         <item>
///             It is internal to <c>Orleans.Streaming</c>, so it cannot be decorated with
///             <c>[TypeMap]</c> and is therefore invisible to the attribute scanning type mapper that
///             <c>KurrentDBGrainStorage</c> uses to derive the KurrentDB event type name.
///         </item>
///         <item>
///             It is an Orleans <c>[GenerateSerializer]</c> type whose members are also internal, so Orleans'
///             own serializer round-trips it correctly while a general purpose JSON serializer may not.
///         </item>
///     </list>
///     <para>
///         Both differences are resolved per storage provider through keyed registrations rather than by
///         special casing inside <c>KurrentDBGrainStorage</c>, which stays free of any knowledge of Orleans'
///         streaming internals. Because <c>KurrentDBGrainStorageOptions.GrainStorageSerializer</c> and
///         <c>TypeMapper</c> are resolved by <c>ConnectionName</c>, these registrations affect only this store.
///     </para>
///     <para>
///         Pub/sub subscription state is rebuildable cluster bookkeeping. Persisting it couples stored data to
///         an Orleans internal type, so a future Orleans release that changes the shape of
///         <c>PubSubGrainState</c> may leave previously written streams unreadable. Orleans' serializer is
///         version tolerant, which keeps the practical risk low. Hosts that would rather not take on that
///         coupling can continue to use an alternative provider, for example <c>AddMemoryGrainStorage</c>.
///     </para>
/// </remarks>
public static class KurrentDBPubSubStoreServiceCollectionExtensions
{
    /// <summary>
    ///     The grain storage provider name Orleans reserves for streaming pub/sub subscription state.
    /// </summary>
    public const string PubSubStoreName = "PubSubStore";

    /// <summary>
    ///     The assembly qualified name of the Orleans streaming pub/sub state type. It is resolved by name
    ///     because the type is internal to <c>Orleans.Streaming</c>.
    /// </summary>
    private const string PubSubGrainStateTypeName = "Orleans.Streams.PubSubGrainState, Orleans.Streaming";

    /// <summary>
    ///     Configures KurrentDB as the store for Orleans streaming pub/sub subscription state.
    /// </summary>
    /// <param name="builder">The silo builder.</param>
    /// <param name="configureOptions">Configures the grain storage options for the pub/sub store.</param>
    /// <remarks>
    ///     The caller is still responsible for registering the keyed <c>KurrentDBClient</c> for the
    ///     <c>ConnectionName</c> chosen in <paramref name="configureOptions" />.
    /// </remarks>
    public static ISiloBuilder AddKurrentDBPubSubStore(this ISiloBuilder builder,
        Action<KurrentDBGrainStorageOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
        return builder.ConfigureServices(services => services.AddKurrentDBPubSubStore(configureOptions));
    }

    /// <summary>
    ///     Configures KurrentDB as the store for Orleans streaming pub/sub subscription state.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Configures the grain storage options for the pub/sub store.</param>
    /// <remarks>
    ///     The caller is still responsible for registering the keyed <c>KurrentDBClient</c> for the
    ///     <c>ConnectionName</c> chosen in <paramref name="configureOptions" />.
    /// </remarks>
    public static IServiceCollection AddKurrentDBPubSubStore(this IServiceCollection services,
        Action<KurrentDBGrainStorageOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));

        // The connection name drives the keyed lookups below, so it is read back from the configured options
        // rather than assumed to match the provider name.
        var options = new KurrentDBGrainStorageOptions();
        configureOptions(options);
        var connectionName = options.ConnectionName;
        if (string.IsNullOrWhiteSpace(connectionName))
        {
            throw new ArgumentException($"{nameof(KurrentDBGrainStorageOptions.ConnectionName)} must be set when configuring the KurrentDB pub/sub store.", nameof(configureOptions));
        }
        var typeMapKinds = options.TypeMapKinds;

        // Orleans' own serializer handles the [GenerateSerializer] pub/sub state type, including its internal
        // members. Resolved per provider, so other stores keep their configured serializer.
        var serializerDescriptor = ServiceDescriptor.KeyedSingleton<IGrainStorageSerializer>(connectionName, (sp, _) =>
        {
            return new OrleansGrainStorageSerializer(sp.GetRequiredService<Serializer>());
        });

        // A dedicated type mapper carries the explicit pub/sub mapping so it never leaks into stores that
        // persist application state.
        var typeMapperDescriptor = ServiceDescriptor.KeyedSingleton<ITypeMapper>(typeMapKinds.ToTypeMapperKey(connectionName), (_, _) =>
        {
            var pubSubStateType = Type.GetType(PubSubGrainStateTypeName)
                ?? throw new InvalidOperationException($"Could not resolve '{PubSubGrainStateTypeName}'. The referenced Orleans version may have moved or renamed this type, so {nameof(AddKurrentDBPubSubStore)} needs to be updated.");
            var typeMapper = new DefaultTypeMapAttributeMapper();
            typeMapper.AddType(pubSubStateType, pubSubStateType.Name);
            return typeMapper;
        });
        services.Add(serializerDescriptor);
        services.Add(typeMapperDescriptor);

        // The serializer and type mapper above are keyed by connection name, so the pub/sub store must not share
        // it with anything else. Checked at silo startup, once every registration has been made.
        var ownDescriptors = new HashSet<ServiceDescriptor> { serializerDescriptor, typeMapperDescriptor };
        services.AddTransient<IConfigurationValidator>(sp => new KurrentDBPubSubStoreValidator(sp, services, connectionName, ownDescriptors));

        return services.AddKurrentDBGrainStorage(PubSubStoreName, configureOptions);
    }
}
