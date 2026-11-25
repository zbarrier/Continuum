using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.Serialization.Orleans;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Serialization;
using Orleans.Storage;

namespace Continuum.Persistence.Orleans.KurrentDB.Tests;

public class KurrentDBPubSubStoreTests
{
    private const string Connection = "kurrentdb";
    private const string AppStorage = "app-storage";

    [Fact]
    public void Shares_A_Connection_With_Grain_Storage_While_Each_Keeps_Its_Own_Serializer_And_Type_Mapper()
    {
        var services = new ServiceCollection().AddSerializer();
        services.AddDefaultTypeMapAttributeMappers(Connection, TypeMapKinds.Snapshot);
        services.AddTypeMappedJsonGrainStorageSerializer(Connection);
        services.AddKurrentDBGrainStorage(AppStorage, options => options.ConnectionName = Connection);
        services.AddKurrentDBPubSubStore(options => options.ConnectionName = Connection);
        using var provider = services.BuildServiceProvider();
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<KurrentDBGrainStorageOptions>>();

        var app = optionsMonitor.Get(AppStorage);
        var pubSub = optionsMonitor.Get(KurrentDBPubSubStoreServiceCollectionExtensions.PubSubStoreName);

        Assert.Equal(Connection, pubSub.ConnectionName);
        Assert.Equal(KurrentDBPubSubStoreServiceCollectionExtensions.PubSubStoreName, pubSub.TypeMapperKey);
        Assert.IsType<OrleansGrainStorageSerializer>(pubSub.GrainStorageSerializer);
        Assert.True(pubSub.TypeMapper.TryGetType("PubSubGrainState", out _));

        Assert.Equal(Connection, app.TypeMapperKey);
        Assert.IsType<TypeMappedJsonGrainStorageSerializer>(app.GrainStorageSerializer);
        Assert.Same(provider.GetRequiredKeyedService<ITypeMapper>(TypeMapKinds.Snapshot.ToTypeMapperKey(Connection)), app.TypeMapper);
        Assert.False(app.TypeMapper.TryGetType("PubSubGrainState", out _));
    }
}
