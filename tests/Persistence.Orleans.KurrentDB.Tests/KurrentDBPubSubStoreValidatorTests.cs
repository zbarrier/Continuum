using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Configuration;
using Orleans.Serialization;

namespace Continuum.Persistence.Orleans.KurrentDB.Tests;

public class KurrentDBPubSubStoreValidatorTests
{
    private const string AppConnection = "app";
    private const string PubSubConnection = "pubsub";

    private static IServiceCollection NewServices() => new ServiceCollection().AddSerializer();

    private static void Validate(IServiceCollection services)
    {
        using var provider = services.BuildServiceProvider();
        foreach (var validator in provider.GetServices<IConfigurationValidator>())
        {
            validator.ValidateConfiguration();
        }
    }

    private static IServiceCollection AppStorage(string connectionName)
    {
        var services = NewServices();
        services.AddDefaultTypeMapAttributeMappers(connectionName, TypeMapKinds.Snapshot);
        services.AddTypeMappedJsonGrainStorageSerializer(connectionName);
        services.AddKurrentDBGrainStorage("app-storage", options => options.ConnectionName = connectionName);
        return services;
    }

    [Fact]
    public void Separate_Connection_Names_Pass()
    {
        var services = AppStorage(AppConnection);
        services.AddKurrentDBPubSubStore(options => options.ConnectionName = PubSubConnection);

        Validate(services);
    }

    [Fact]
    public void Pub_Sub_Store_On_Its_Own_Passes()
    {
        var services = NewServices();
        services.AddKurrentDBPubSubStore(options => options.ConnectionName = PubSubConnection);

        Validate(services);
    }

    [Fact]
    public void Connection_Name_Shared_With_Grain_Storage_Is_Rejected()
    {
        var services = AppStorage(AppConnection);
        services.AddKurrentDBPubSubStore(options => options.ConnectionName = AppConnection);

        var exception = Assert.Throws<OrleansConfigurationException>(() => Validate(services));
        Assert.Contains("app-storage", exception.Message);
    }

    [Fact]
    public void Connection_Name_Shared_With_Other_Registrations_Is_Rejected()
    {
        var services = NewServices();
        services.AddTypeMappedJsonGrainStorageSerializer(PubSubConnection);
        services.AddKurrentDBPubSubStore(options => options.ConnectionName = PubSubConnection);

        Assert.Throws<OrleansConfigurationException>(() => Validate(services));
    }
}
