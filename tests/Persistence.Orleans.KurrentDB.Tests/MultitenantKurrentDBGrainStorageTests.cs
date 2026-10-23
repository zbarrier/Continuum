using Continuum.Orleans.Tenancy;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Storage;
using Orleans.TestingHost;

namespace Continuum.Persistence.Orleans.KurrentDB.Tests;

[Collection(ClusterCollection.Name)]
public class MultitenantKurrentDBGrainStorageTests(ClusterFixture fixture)
{
    private const string GrainTypeName = "counter";

    private IServiceProvider SiloServices => (fixture.Cluster.Primary as InProcessSiloHandle
        ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services;

    private IGrainStorage Storage => SiloServices.GetRequiredKeyedService<IGrainStorage>(TestSiloConfigurations.MultitenantProvider);

    private KurrentDBClient Client => SiloServices.GetRequiredKeyedService<KurrentDBClient>(TestSiloConfigurations.ConnectionName);

    private static GrainId GrainFor(string? tenantId) =>
        GrainId.Create(GrainTypeName, TenantKey.Create(tenantId, Guid.NewGuid().ToString("N")));

    private static GrainState<CounterState> NewState(int count = 0) => new(new CounterState { Count = count });

    private async Task<long> CountEventsAsync(GrainId grainId)
    {
        var result = Client.ReadStreamAsync(Direction.Forwards, $"state__{grainId.Type}-{grainId.Key}", StreamPosition.Start,
            cancellationToken: TestContext.Current.CancellationToken);
        if (await result.ReadState == ReadState.StreamNotFound)
        {
            return 0;
        }
        return await result.CountAsync(TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("shared-tenant")]
    [InlineData(TestSiloConfigurations.DedicatedTenantId)]
    public async Task Should_Write_And_Read_State_In_Tenant_Qualified_Stream(string? tenantId)
    {
        var grainId = GrainFor(tenantId);

        await Storage.WriteStateAsync(GrainTypeName, grainId, NewState(7));

        var read = NewState();
        await Storage.ReadStateAsync(GrainTypeName, grainId, read);
        Assert.True(read.RecordExists);
        Assert.Equal(7, read.State!.Count);
        Assert.Equal(1, await CountEventsAsync(grainId));
        if (tenantId is not null)
        {
            Assert.StartsWith($"{tenantId}|", grainId.Key.ToString());
        }
    }

    [Fact]
    public async Task Should_Isolate_State_Between_Tenants_With_The_Same_Key()
    {
        var key = Guid.NewGuid().ToString("N");
        var acme = GrainId.Create(GrainTypeName, TenantKey.Create("acme", key));
        var contoso = GrainId.Create(GrainTypeName, TenantKey.Create("contoso", key));

        await Storage.WriteStateAsync(GrainTypeName, acme, NewState(1));
        await Storage.WriteStateAsync(GrainTypeName, contoso, NewState(2));

        var acmeState = NewState();
        var contosoState = NewState();
        await Storage.ReadStateAsync(GrainTypeName, acme, acmeState);
        await Storage.ReadStateAsync(GrainTypeName, contoso, contosoState);
        Assert.Equal(1, acmeState.State!.Count);
        Assert.Equal(2, contosoState.State!.Count);
    }
}
