using Microsoft.Extensions.DependencyInjection;

using Orleans.Serialization;

namespace Continuum.Orleans.KurrentDB;

/// <summary>
///     Lets Orleans serialize the KurrentDB client exceptions that Continuum providers attach as inner exceptions.
/// </summary>
public static class KurrentDBExceptionSerializationServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the KurrentDB client and gRPC namespaces to <see cref="ExceptionSerializationOptions.SupportedNamespacePrefixes" />.
    /// </summary>
    /// <remarks>
    ///     Orleans only serializes exception types from allowed namespaces. Without this, a storage failure that wraps
    ///     a KurrentDB or gRPC exception reaches the caller as a <see cref="CodecNotFoundException" /> instead of the
    ///     original error, so grains calling into storage-backed grains (for example the pub/sub grain) can log the real cause.
    ///     This is for logging within the silo; business grains are expected to catch exceptions, log them, and return an
    ///     error in a Result, so exception details are not meant to reach clients.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKurrentDBExceptionSerialization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.Configure<ExceptionSerializationOptions>(options =>
        {
            options.SupportedNamespacePrefixes.Add("KurrentDB.Client");
            options.SupportedNamespacePrefixes.Add("Grpc.Core");
        });
        return services;
    }
}
