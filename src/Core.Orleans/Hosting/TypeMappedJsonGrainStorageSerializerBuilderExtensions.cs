using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Continuum.Converters.Json;
using Continuum.Serialization.Orleans;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orleans.Storage;

namespace Continuum.Hosting.Orleans;

/// <summary>
/// Registers <see cref="TypeMappedJsonGrainStorageSerializer"/> with dependency injection.
/// </summary>
public static class TypeMappedJsonGrainStorageSerializerBuilderExtensions
{
    /// <summary>
    /// Registers a keyed <see cref="IGrainStorageSerializer"/> for <paramref name="connectionName"/> backed by <see cref="TypeMappedJsonGrainStorageSerializer"/>.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="connectionName">The service key for the serializer.</param>
    /// <param name="includeDefaultConverters">Whether to add <see cref="NewIdConverter"/>.</param>
    /// <param name="typeInfoResolver">Optional JSON metadata resolver, such as a <see cref="JsonSerializerContext"/>. Required for Native AOT.</param>
    /// <param name="jsonConverters">Additional converters.</param>
    /// <returns>The host application builder.</returns>
    public static IHostApplicationBuilder AddTypeMappedJsonGrainStorageSerializer(this IHostApplicationBuilder builder,
        string connectionName,
        bool includeDefaultConverters = true,
        IJsonTypeInfoResolver? typeInfoResolver = null,
        params JsonConverter[] jsonConverters)
    {
        _ = builder.Services.AddTypeMappedJsonGrainStorageSerializer(connectionName, includeDefaultConverters, typeInfoResolver, jsonConverters);

        return builder;
    }

    /// <inheritdoc cref="AddTypeMappedJsonGrainStorageSerializer(IHostApplicationBuilder, string, bool, IJsonTypeInfoResolver?, JsonConverter[])"/>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionName">The service key for the serializer.</param>
    /// <param name="includeDefaultConverters">Whether to add <see cref="NewIdConverter"/>.</param>
    /// <param name="typeInfoResolver">Optional JSON metadata resolver, such as a <see cref="JsonSerializerContext"/>. Required for Native AOT.</param>
    /// <param name="jsonConverters">Additional converters.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddTypeMappedJsonGrainStorageSerializer(this IServiceCollection services,
        string connectionName,
        bool includeDefaultConverters = true,
        IJsonTypeInfoResolver? typeInfoResolver = null,
        params JsonConverter[] jsonConverters)
    {
        services.AddKeyedSingleton<IGrainStorageSerializer>(connectionName, (sp, cn) =>
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            if (includeDefaultConverters)
            {
                options.Converters.Add(new NewIdConverter());
            }
            foreach (var converter in jsonConverters)
            {
                options.Converters.Add(converter);
            }
            return new TypeMappedJsonGrainStorageSerializer(options, typeInfoResolver);
        });

        return services;
    }
}
