using Continuum.EventSourcing.Orleans.CosmosDB.Tests.Events;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

using Orleans.Storage;
using Orleans.TestingHost;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests;

[Collection(ClusterCollection.Name)]
public class LogConsistentStorageTests(ClusterFixture fixture)
{
    private const string GrainTypeName = "snack";

    private IServiceProvider SiloServices => (fixture.Cluster.Primary as InProcessSiloHandle
        ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services;

    private ILogConsistentStorage Storage => SiloServices.GetRequiredService<ILogConsistentStorage>();

    private static GrainId NewGrainId() => GrainId.Create(GrainTypeName, Guid.NewGuid().ToString("N"));

    private static SnackEvent[] CreateEvents(int count)
    {
        var id = Guid.NewGuid();
        var events = new SnackEvent[count];
        events[0] = new SnackInitializedEvent(id, "Name0", Guid.NewGuid(), DateTimeOffset.UtcNow, "Test", 1);
        for (var index = 1; index < count; index++)
        {
            events[index] = new SnackNameChangedEvent(id, $"Name{index}", Guid.NewGuid(), DateTimeOffset.UtcNow, "Test", index + 1);
        }
        return events;
    }

    [Fact]
    public async Task Should_Report_Zero_Version_And_No_Events_For_Missing_Stream()
    {
        var grainId = NewGrainId();

        Assert.Equal(0, await Storage.GetLastVersionAsync(GrainTypeName, grainId));
        Assert.Empty(await Storage.ReadAsync<SnackEvent>(GrainTypeName, grainId, 0, 10));
    }

    [Fact]
    public async Task Should_Return_Event_Count_After_Appends()
    {
        var grainId = NewGrainId();
        var events = CreateEvents(5);

        Assert.Equal(3, await Storage.AppendAsync(GrainTypeName, grainId, events[..3], 0));
        Assert.Equal(5, await Storage.AppendAsync(GrainTypeName, grainId, events[3..], 3));
        Assert.Equal(5, await Storage.GetLastVersionAsync(GrainTypeName, grainId));
    }

    [Fact]
    public async Task Should_Return_Current_Version_When_Appending_Nothing()
    {
        var grainId = NewGrainId();
        await Storage.AppendAsync(GrainTypeName, grainId, CreateEvents(2), 0);

        Assert.Equal(2, await Storage.AppendAsync(GrainTypeName, grainId, Array.Empty<SnackEvent>(), 2));
    }

    [Fact]
    public async Task Should_Read_Requested_Range_Using_Event_Counts()
    {
        var grainId = NewGrainId();
        var events = CreateEvents(5);
        await Storage.AppendAsync(GrainTypeName, grainId, events, 0);

        Assert.Equal(events, await Storage.ReadAsync<SnackEvent>(GrainTypeName, grainId, 0, 5));

        // Starting after 1 event and taking 2 returns the events that produce versions 2 and 3.
        Assert.Equal(events[1..3], await Storage.ReadAsync<SnackEvent>(GrainTypeName, grainId, 1, 2));

        Assert.Empty(await Storage.ReadAsync<SnackEvent>(GrainTypeName, grainId, 5, 10));
        Assert.Empty(await Storage.ReadAsync<SnackEvent>(GrainTypeName, grainId, 0, 0));
    }

    [Fact]
    public async Task Should_Reject_Negative_Arguments()
    {
        var grainId = NewGrainId();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Storage.ReadAsync<SnackEvent>(GrainTypeName, grainId, -1, 1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Storage.AppendAsync(GrainTypeName, grainId, CreateEvents(1), -1));
    }

    [Fact]
    public async Task Should_Reject_Appends_Larger_Than_A_Single_Batch()
    {
        var grainId = NewGrainId();

        // The default BatchSize is 100 and the stream header takes one operation.
        await Assert.ThrowsAnyAsync<Exception>(() => Storage.AppendAsync(GrainTypeName, grainId, CreateEvents(100), 0));
        Assert.Equal(0, await Storage.GetLastVersionAsync(GrainTypeName, grainId));

        Assert.Equal(99, await Storage.AppendAsync(GrainTypeName, grainId, CreateEvents(99), 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public async Task Should_Throw_InconsistentState_On_Version_Conflict(int expectedVersion)
    {
        var grainId = NewGrainId();
        await Storage.AppendAsync(GrainTypeName, grainId, CreateEvents(2), 0);

        await Assert.ThrowsAsync<InconsistentStateException>(() => Storage.AppendAsync(GrainTypeName, grainId, CreateEvents(1), expectedVersion));
        Assert.Equal(2, await Storage.GetLastVersionAsync(GrainTypeName, grainId));
    }

    [Fact]
    public async Task Should_Store_Header_And_Events_In_The_Stream_Partition()
    {
        var grainId = NewGrainId();
        await Storage.AppendAsync(GrainTypeName, grainId, CreateEvents(3), 0);

        var client = SiloServices.GetRequiredKeyedService<CosmosClient>(TestSiloConfigurations.ConnectionName);
        var container = client.GetContainer(ClusterFixture.DatabaseName, ClusterFixture.ContainerName);
        var query = new QueryDefinition("SELECT c.id, c.type, c.ver, c.subSeq, c.dataType FROM c WHERE c.streamName = @streamName ORDER BY c.ver ASC")
            .WithParameter("@streamName", $"{fixture.Cluster.Options.ServiceId}/{grainId}");

        var items = new List<StoredItem>();
        using var iterator = container.GetItemQueryIterator<StoredItem>(query);
        while (iterator.HasMoreResults)
        {
            items.AddRange(await iterator.ReadNextAsync(TestContext.Current.CancellationToken));
        }

        var header = Assert.Single(items, i => i.type == "hdr");
        Assert.Equal("hdr", header.id);
        Assert.Equal(3UL, header.ver);

        var events = items.Where(i => i.type == "evt").ToList();
        Assert.Equal([1UL, 2UL, 3UL], events.Select(e => e.ver));
        Assert.Equal([0UL, 1UL, 2UL], events.Select(e => e.subSeq));
        Assert.All(events, e => Assert.False(string.IsNullOrEmpty(e.dataType)));
    }

    [Fact]
    public async Task Should_Reject_Null_Entries_On_Append()
    {
        var grainId = NewGrainId();
        await Storage.AppendAsync(GrainTypeName, grainId, CreateEvents(1), 0);

        SnackEvent[] entries = [CreateEvents(1)[0], null!];
        await Assert.ThrowsAsync<CosmosDBLogConsistentStorageException>(() => Storage.AppendAsync(GrainTypeName, grainId, entries, 1));
        Assert.Equal(1, await Storage.GetLastVersionAsync(GrainTypeName, grainId));
    }

    [Fact]
    public async Task Should_Throw_When_Reading_An_Unregistered_Event_Type()
    {
        var grainId = NewGrainId();
        await InsertEventItemAsync(grainId, "Tests.UnregisteredEvent", new { });

        await Assert.ThrowsAsync<CosmosDBLogConsistentStorageException>(() => Storage.ReadAsync<SnackEvent>(GrainTypeName, grainId, 0, 1));
    }

    [Fact]
    public async Task Should_Throw_When_A_Stored_Event_Reads_As_Null()
    {
        var grainId = NewGrainId();
        await InsertEventItemAsync(grainId, "Tests.SnackInitializedEvent", null);

        await Assert.ThrowsAsync<CosmosDBLogConsistentStorageException>(() => Storage.ReadAsync<SnackEvent>(GrainTypeName, grainId, 0, 1));
    }

    private async Task InsertEventItemAsync(GrainId grainId, string dataType, object? data)
    {
        var streamName = $"{fixture.Cluster.Options.ServiceId}/{grainId}";
        var client = SiloServices.GetRequiredKeyedService<CosmosClient>(TestSiloConfigurations.ConnectionName);
        var container = client.GetContainer(ClusterFixture.DatabaseName, ClusterFixture.ContainerName);
        var item = new { id = Guid.NewGuid().ToString(), type = "evt", streamName, ver = 1UL, subSeq = 0UL, dataType, data };
        await container.CreateItemAsync(item, new PartitionKey(streamName), cancellationToken: TestContext.Current.CancellationToken);
    }

    // Lower-case property names match the stored JSON so the SDK's default serializer can read the projection.
    private sealed record StoredItem(string id, string type, ulong ver, ulong subSeq, string dataType);
}
