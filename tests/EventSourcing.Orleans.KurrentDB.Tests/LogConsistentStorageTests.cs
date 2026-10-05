using Continuum.EventSourcing.Orleans.KurrentDB.Abstractions;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Events;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Storage;
using Orleans.TestingHost;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests;

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
    public async Task Should_Record_Transaction_Metadata_For_Each_Append()
    {
        var grainId = NewGrainId();
        var events = CreateEvents(4);
        await Storage.AppendAsync(GrainTypeName, grainId, events[..3], 0);
        await Storage.AppendAsync(GrainTypeName, grainId, events[3..], 3);

        var client = SiloServices.GetRequiredKeyedService<KurrentDBClient>("journaledGrainLog");
        var metadata = await client.ReadStreamAsync(Direction.Forwards, $"{grainId.Type}-{grainId.Key}", StreamPosition.Start, cancellationToken: TestContext.Current.CancellationToken)
            .Select(e => KurrentDBEventMetadataCodec.Read(e.Event.Metadata.Span))
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(4, metadata.Count);
        var first = metadata[..3];
        Assert.All(first, m => Assert.Equal(first[0].TransactionId, m.TransactionId));
        Assert.All(first, m => Assert.Equal(3, m.TransactionSize));
        Assert.All(first, m => Assert.Equal(3, m.TransactionPartitionSize));
        Assert.Equal([0, 1, 2], first.Select(m => m.TransactionPartitionIndex));

        var last = metadata[3];
        Assert.NotNull(last.TransactionId);
        Assert.NotEqual(first[0].TransactionId, last.TransactionId);
        Assert.Equal(1, last.TransactionSize);
        Assert.Equal(1, last.TransactionPartitionSize);
        Assert.Equal(0, last.TransactionPartitionIndex);
    }
}
