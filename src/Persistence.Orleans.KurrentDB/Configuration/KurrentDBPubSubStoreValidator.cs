using Continuum;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Hosting;
using Orleans.Storage;

namespace Orleans.Configuration;

/// <summary>
///     Rejects a pub/sub store whose connection name is used by anything else. The pub/sub store registers its
///     own serializer and type mapper under its connection name, so anything sharing that name would silently
///     use them instead of its own.
/// </summary>
internal sealed class KurrentDBPubSubStoreValidator : IConfigurationValidator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceCollection _services;
    private readonly string _connectionName;
    private readonly IReadOnlySet<ServiceDescriptor> _ownDescriptors;

    public KurrentDBPubSubStoreValidator(IServiceProvider serviceProvider, IServiceCollection services,
        string connectionName, IReadOnlySet<ServiceDescriptor> ownDescriptors)
    {
        _serviceProvider = serviceProvider;
        _services = services;
        _connectionName = connectionName;
        _ownDescriptors = ownDescriptors;
    }

    public void ValidateConfiguration()
    {
        var optionsMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<KurrentDBGrainStorageOptions>>();
        foreach (var registration in _serviceProvider.GetServices<KurrentDBGrainStorageRegistration>())
        {
            if (registration.Name == KurrentDBPubSubStoreServiceCollectionExtensions.PubSubStoreName)
            {
                continue;
            }
            if (string.Equals(optionsMonitor.Get(registration.Name).ConnectionName, _connectionName, StringComparison.Ordinal))
            {
                throw new OrleansConfigurationException($"Invalid configuration for the KurrentDB pub/sub store. Its connection name '{_connectionName}' is also used by the KurrentDB grain storage provider '{registration.Name}'. Give the pub/sub store its own connection name.");
            }
        }
        foreach (var descriptor in _services)
        {
            if (descriptor.IsKeyedService
                && (descriptor.ServiceType == typeof(IGrainStorageSerializer) || descriptor.ServiceType == typeof(Continuum.TypeMapping.ITypeMapper))
                && descriptor.ServiceKey is string key
                && IsKeyForConnection(descriptor, key)
                && !_ownDescriptors.Contains(descriptor))
            {
                throw new OrleansConfigurationException($"Invalid configuration for the KurrentDB pub/sub store. Another {descriptor.ServiceType.Name} is registered for its connection name '{_connectionName}', and the pub/sub store would replace it. Give the pub/sub store its own connection name.");
            }
        }
    }

    private bool IsKeyForConnection(ServiceDescriptor descriptor, string key)
    {
        if (descriptor.ServiceType == typeof(IGrainStorageSerializer))
        {
            return key == _connectionName;
        }
        foreach (var kind in Enum.GetValues<Continuum.TypeMapping.TypeMapKinds>())
        {
            if (kind.ToTypeMapperKey(_connectionName) == key)
            {
                return true;
            }
        }
        return false;
    }
}
