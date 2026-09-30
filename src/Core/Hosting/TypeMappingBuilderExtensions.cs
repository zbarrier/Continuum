using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Continuum.Hosting;

/// <summary>
/// Registers attribute-based <see cref="ITypeMapper"/> instances with dependency injection.
/// </summary>
public static class TypeMappingBuilderExtensions
{
    /// <summary>
    /// Registers keyed <see cref="ITypeMapper"/> singletons for <paramref name="connectionName"/>, populated from attribute-decorated types.
    /// </summary>
    /// <remarks>
    /// A mapper is registered for every non-empty combination of the kinds in <paramref name="typeMapKinds"/>, keyed by
    /// <see cref="TypeMapKindsExtensions.ToTypeMapperKey(TypeMapKinds, string)"/>. The mapper for the full combination is also
    /// registered under <paramref name="connectionName"/> itself. That mapper is built immediately, so registration errors
    /// surface at startup; the others are built on first resolution. All mappers are frozen
    /// (see <see cref="ITypeMapper.Freeze"/>).
    /// </remarks>
    /// <param name="builder">The host application builder.</param>
    /// <param name="connectionName">The connection name to associate with the type mapper.</param>
    /// <param name="typeMapKinds">The kinds to register. A mapper is registered for every combination of the included kinds.</param>
    /// <returns>The host application builder.</returns>
    public static IHostApplicationBuilder AddDefaultTypeMapAttributeMappers(this IHostApplicationBuilder builder, string connectionName,
        TypeMapKinds typeMapKinds = TypeMapKinds.Command | TypeMapKinds.DomainEvent | TypeMapKinds.IntegrationEvent | TypeMapKinds.Snapshot | TypeMapKinds.Metadata)
    {
        _ = builder.Services.AddDefaultTypeMapAttributeMappers(connectionName, typeMapKinds);
        return builder;
    }

    /// <inheritdoc cref="AddDefaultTypeMapAttributeMappers(IHostApplicationBuilder, string, TypeMapKinds)"/>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionName">The connection name to associate with the type mappers.</param>
    /// <param name="typeMapKinds">The kinds to register. A mapper is registered for every combination of the included kinds.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDefaultTypeMapAttributeMappers(this IServiceCollection services, string connectionName,
        TypeMapKinds typeMapKinds = TypeMapKinds.Command | TypeMapKinds.DomainEvent | TypeMapKinds.IntegrationEvent | TypeMapKinds.Snapshot | TypeMapKinds.Metadata)
    {
        // The mapper for the full combination is always used, so build it now: registration errors (such as duplicate
        // type names) surface at startup, and it is shared by the full-combination key and the connection name key.
        var defaultMapper = CreateMapper(typeMapKinds);
        services.AddKeyedSingleton<ITypeMapper>(typeMapKinds.ToTypeMapperKey(connectionName), defaultMapper);
        services.AddKeyedSingleton<ITypeMapper>(connectionName, defaultMapper);

        // Register a mapper for every other non-empty flag combination contained in typeMapKinds by walking its
        // submasks directly instead of testing every integer up to the value. These are rarely used, so they are
        // built on first resolution.
        var all = (int)typeMapKinds;
        for (var subset = (all - 1) & all; subset > 0; subset = (subset - 1) & all)
        {
            var currTypeMapKinds = (TypeMapKinds)subset;
            services.AddKeyedSingleton<ITypeMapper>(currTypeMapKinds.ToTypeMapperKey(connectionName), (_, _) => CreateMapper(currTypeMapKinds));
        }

        return services;
    }

    private static DefaultTypeMapAttributeMapper CreateMapper(TypeMapKinds typeMapKinds)
    {
        var mapper = new DefaultTypeMapAttributeMapper();
        mapper.RegisterKnownTypes(typeMapKinds);
        mapper.Freeze();
        return mapper;
    }
}
