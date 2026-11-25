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
///         streaming internals. The registrations use the store's own <c>TypeMapperKey</c>,
///         <see cref="PubSubStoreName" />, so they affect only this store, and it can share a <c>ConnectionName</c>
///         (and so a <c>KurrentDBClient</c>) with other providers.
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
    ///     The grain storage provider name Orleans reserves for streaming pub/sub subscription state. It is also the
    ///     <c>TypeMapperKey</c> the pub/sub store's serializer and type mapper are registered under.
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
    ///     <c>ConnectionName</c> chosen in <paramref name="configureOptions" />. Any <c>TypeMapperKey</c> set there is
    ///     replaced with <see cref="PubSubStoreName" />.
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
    ///     <c>ConnectionName</c> chosen in <paramref name="configureOptions" />. Any <c>TypeMapperKey</c> set there is
    ///     replaced with <see cref="PubSubStoreName" />.
    /// </remarks>
    public static IServiceCollection AddKurrentDBPubSubStore(this IServiceCollection services,
        Action<KurrentDBGrainStorageOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));

        // The type mapper is keyed by the configured kinds, so they are read back from the configured options.
        var configured = new KurrentDBGrainStorageOptions();
        configureOptions(configured);
        var typeMapKinds = configured.TypeMapKinds;

        // Orleans' own serializer handles the [GenerateSerializer] pub/sub state type, including its internal
        // members. Keyed by the store's own type mapper key, so other stores keep their configured serializer.
        services.AddKeyedSingleton<IGrainStorageSerializer>(PubSubStoreName, (sp, _) =>
        {
            return new OrleansGrainStorageSerializer(sp.GetRequiredService<Serializer>());
        });

        // A dedicated type mapper carries the explicit pub/sub mapping so it never leaks into stores that
        // persist application state.
        services.AddKeyedSingleton<ITypeMapper>(typeMapKinds.ToTypeMapperKey(PubSubStoreName), (_, _) =>
        {
            var pubSubStateType = Type.GetType(PubSubGrainStateTypeName)
                ?? throw new InvalidOperationException($"Could not resolve '{PubSubGrainStateTypeName}'. The referenced Orleans version may have moved or renamed this type, so {nameof(AddKurrentDBPubSubStore)} needs to be updated.");
            var typeMapper = new DefaultTypeMapAttributeMapper();
            typeMapper.AddType(pubSubStateType, pubSubStateType.Name);
            return typeMapper;
        });

        return services.AddKurrentDBGrainStorage(PubSubStoreName, options =>
        {
            configureOptions(options);
            options.TypeMapperKey = PubSubStoreName;
        });
    }
}
