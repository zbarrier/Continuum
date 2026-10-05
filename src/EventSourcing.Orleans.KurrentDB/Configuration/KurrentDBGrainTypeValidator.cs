using Continuum.EventSourcing.Orleans.KurrentDB;

using Microsoft.Extensions.DependencyInjection;

using Orleans.EventSourcing;
using Orleans.Metadata;
using Orleans.Providers;

namespace Orleans.Configuration;

/// <summary>
///     Rejects journaled grains stored by a KurrentDB provider whose grain type contains '-'.
/// </summary>
/// <remarks>
///     KurrentDB derives a stream's category from the text before the first '-'. The default stream name is
///     <c>{GrainType}-{GrainKey}</c>, so such a grain's events would be filed under a truncated category.
/// </remarks>
public sealed class KurrentDBGrainTypeValidator : IConfigurationValidator
{
    private const char CategorySeparator = '-';

    private readonly IServiceProvider _serviceProvider;
    private readonly string _name;

    /// <summary>
    ///     Creates a validator for the named KurrentDB log-consistency provider.
    /// </summary>
    /// <param name="serviceProvider">The silo service provider, used to resolve the grain manifest.</param>
    /// <param name="name">The provider name.</param>
    public KurrentDBGrainTypeValidator(IServiceProvider serviceProvider, string name)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _name = name;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        var manifestProvider = _serviceProvider.GetService<IClusterManifestProvider>();
        var classMap = _serviceProvider.GetService<GrainClassMap>();
        if (manifestProvider is null || classMap is null)
        {
            return;
        }
        var offending = FindOffendingGrainTypes(EnumerateGrainClasses(manifestProvider, classMap), _name);
        if (offending.Count > 0)
        {
            throw new OrleansConfigurationException($"Invalid configuration for {nameof(KurrentDBLogConsistentStorage)} with name {_name}. The grain types {string.Join(", ", offending)} contain '-', which KurrentDB uses to separate the stream category. Remove '-' from the grain type.");
        }
    }

    private static IEnumerable<(string GrainType, Type GrainClass)> EnumerateGrainClasses(IClusterManifestProvider manifestProvider, GrainClassMap classMap)
    {
        foreach (var grainType in manifestProvider.LocalGrainManifest.Grains.Keys)
        {
            if (classMap.TryGetGrainClass(grainType, out var grainClass))
            {
                yield return (grainType.ToString()!, grainClass);
            }
        }
    }

    /// <summary>
    ///     Returns the journaled grains stored by the named provider whose grain type contains '-', formatted for an error message.
    /// </summary>
    internal static List<string> FindOffendingGrainTypes(IEnumerable<(string GrainType, Type GrainClass)> grains, string providerName)
    {
        var offending = new List<string>();
        foreach (var (grainType, grainClass) in grains)
        {
            if (grainType.IndexOf(CategorySeparator) >= 0 && IsStoredByProvider(grainClass, providerName))
            {
                offending.Add($"{grainType} ({grainClass.FullName})");
            }
        }
        offending.Sort(StringComparer.Ordinal);
        return offending;
    }

    private static bool IsStoredByProvider(Type grainClass, string name)
    {
        if (!IsJournaledGrain(grainClass))
        {
            return false;
        }
        var attribute = (LogConsistencyProviderAttribute?)Attribute.GetCustomAttribute(grainClass, typeof(LogConsistencyProviderAttribute), inherit: true);
        var providerName = attribute?.ProviderName ?? ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME;
        return string.Equals(providerName, name, StringComparison.Ordinal);
    }

    private static bool IsJournaledGrain(Type grainClass)
    {
        for (var current = grainClass.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(JournaledGrain<,>))
            {
                return true;
            }
        }
        return false;
    }
}
