using System.Text.Json;
using System.Text.Json.Serialization;

using Continuum.Converters.Json;
using Continuum.Streaming;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Continuum.Hosting;

public static class SystemTextJsonStreamedEventSerdeBuilderExtensions
{
    public static IHostApplicationBuilder AddSystemTextJsonStreamEventSerde(this IHostApplicationBuilder builder, string connectionName,
        bool includeDefaultConvertors = true, params JsonConverter[] jsonConverters)
    {
        _ = builder.Services.AddSystemTextJsonStreamEventSerde(connectionName, includeDefaultConvertors, jsonConverters);
        return builder;
    }

    /// <summary>
    ///     Registers a keyed <see cref="IStreamedEventSerde" /> for the supplied connection name.
    /// </summary>
    /// <remarks>
    ///     This overload exists for hosts that only have an <see cref="IServiceCollection" />, such as an Orleans
    ///     silo configured through <c>ISiloBuilder.ConfigureServices</c> or a test cluster silo configurator.
    /// </remarks>
    public static IServiceCollection AddSystemTextJsonStreamEventSerde(this IServiceCollection services, string connectionName,
        bool includeDefaultConvertors = true, params JsonConverter[] jsonConverters)
    {
        services.AddKeyedSingleton<IStreamedEventSerde>(connectionName, (sp, cn) =>
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            if (includeDefaultConvertors)
            {
                options.Converters.Add(new NewIdConverter());
            }
            foreach (var converter in jsonConverters)
            {
                options.Converters.Add(converter);
            }
            return new SystemTextJsonStreamEventSerde(options);
        });

        return services;
    }
}
