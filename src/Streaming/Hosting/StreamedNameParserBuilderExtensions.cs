using System.Diagnostics.CodeAnalysis;

using Continuum.Streaming;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Continuum.Hosting;

/// <summary>
/// Registers keyed <see cref="IStreamedNameParser" /> implementations.
/// </summary>
public static class StreamedNameParserBuilderExtensions
{
    /// <summary>
    ///     Registers a keyed <see cref="IStreamedNameParser" /> for the supplied connection name.
    /// </summary>
    public static IHostApplicationBuilder AddStreamedNameParser<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParser>(this IHostApplicationBuilder builder, string connectionName)
        where TParser : class, IStreamedNameParser
    {
        _ = builder.Services.AddStreamedNameParser<TParser>(connectionName);
        return builder;
    }

    /// <summary>
    ///     Registers a keyed <see cref="IStreamedNameParser" /> for the supplied connection name.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The parser is keyed by connection rather than registered once, because how a stream name splits is a
    ///         property of the store and the application writing to it. An application reading KurrentDB names of
    ///         the form <c>snack-123</c> and CosmosDB names of the form <c>snack/123</c> needs both at the same
    ///         time, and resolving a single unkeyed parser would silently give one of them the wrong split.
    ///     </para>
    ///     <para>
    ///         This overload exists for hosts that only have an <see cref="IServiceCollection" />, such as an
    ///         Orleans silo configured through <c>ISiloBuilder.ConfigureServices</c> or a test cluster silo
    ///         configurator.
    ///     </para>
    /// </remarks>
    public static IServiceCollection AddStreamedNameParser<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParser>(this IServiceCollection services, string connectionName)
        where TParser : class, IStreamedNameParser
    {
        services.AddKeyedSingleton<IStreamedNameParser, TParser>(connectionName);
        return services;
    }

    /// <summary>
    ///     Registers an already constructed keyed <see cref="IStreamedNameParser" /> for the supplied connection name.
    /// </summary>
    public static IServiceCollection AddStreamedNameParser(this IServiceCollection services, string connectionName, IStreamedNameParser parser)
    {
        ArgumentNullException.ThrowIfNull(parser);
        services.AddKeyedSingleton(connectionName, parser);
        return services;
    }
}
