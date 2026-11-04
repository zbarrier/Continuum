using Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream.Snacks;
using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Cluster;
using Continuum.Orleans.Events;
using Continuum.Orleans.KurrentDB;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;

using Orleans.TestingHost;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Storage;

[Collection(ClusterCollection.Name)]
public class MultiStreamLogConsistentStorageTests(ClusterFixture fixture)
{
    private const string GrainTypeName = "snack";

    private IServiceProvider SiloServices => (fixture.Cluster.Primary as InProcessSiloHandle
        ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services;

    private IMultiStreamLogConsistentStorage Storage => (IMultiStreamLogConsistentStorage)SiloServices.GetRequiredService<ILogConsistentStorage>();

    private static GrainId NewGrainId() => GrainId.Create(GrainTypeName, Guid.NewGuid().ToString("N"));

    private static SnackEvent[] CreateEvents(int count)
    {
        var id = Guid.NewGuid();
        var events = new SnackEvent[count];
        events[0] = new SnackInitializedEvent(id, "Name0", DateTimeOffset.UtcNow);
        for (var index = 1; index < count; index++)
        {
            events[index] = new SnackNameChangedEvent(id, $"Name{index}", DateTimeOffset.UtcNow);
        }
        return events;
    }

    private sealed record GroupedAppend(GrainId GrainId, SnackEvent[] Events, int? ExpectedVersion);

    private static GroupedAppend Append(GrainId grainId, SnackEvent[] events, int? expectedVersion) => new(grainId, events, expectedVersion);

    // Appends each grain's events as one block, in the order of the appends.
    private Task<IReadOnlyList<int>> AppendGroupedAsync(GroupedAppend[] appends, Guid transactionId) => Storage.AppendAsync(
        appends.Select(a => new LogStream(GrainTypeName, a.GrainId, a.ExpectedVersion)).ToArray(),
        appends.SelectMany((a, index) => a.Events.Select(e => new LogRecord<SnackEvent>(index, e))).ToArray(),
        transactionId);

    [Fact]
    public async Task Should_Append_To_Every_Stream_And_Return_Each_Version()
    {
        var first = NewGrainId();
        var second = NewGrainId();
        await Storage.AppendAsync(GrainTypeName, second, CreateEvents(2), 0, Guid.NewGuid());
        var firstEvents = CreateEvents(3);
        var secondEvents = CreateEvents(1);
        var transactionId = Guid.NewGuid();

        var versions = await AppendGroupedAsync([Append(first, firstEvents, 0), Append(second, secondEvents, 2)], transactionId);

        Assert.Equal([3, 3], versions);
        Assert.Equal(firstEvents, await Storage.ReadAsync<SnackEvent>(GrainTypeName, first, 0, 10));
        Assert.Equal(secondEvents, await Storage.ReadAsync<SnackEvent>(GrainTypeName, second, 2, 10));
        Assert.Equal(transactionId, await Storage.GetTransactionIdAsync(GrainTypeName, first, 1));
        Assert.Equal(transactionId, await Storage.GetTransactionIdAsync(GrainTypeName, second, 3));
    }

    [Fact]
    public async Task Should_Record_Transaction_Metadata_Partitioned_By_Stream()
    {
        var first = NewGrainId();
        var second = NewGrainId();
        var transactionId = Guid.NewGuid();

        await AppendGroupedAsync([Append(first, CreateEvents(3), 0), Append(second, CreateEvents(2), 0)], transactionId);

        var firstMetadata = await ReadMetadataAsync(first);
        var secondMetadata = await ReadMetadataAsync(second);
        Assert.All(firstMetadata.Concat(secondMetadata), m =>
        {
            Assert.Equal(transactionId, m.TransactionId);
            Assert.Equal(5, m.TransactionSize);
        });
        Assert.All(firstMetadata, m => Assert.Equal(3, m.TransactionPartitionSize));
        Assert.All(secondMetadata, m => Assert.Equal(2, m.TransactionPartitionSize));
        Assert.Equal([0, 1, 2], firstMetadata.Select(m => m.TransactionPartitionIndex));
        Assert.Equal([0, 1], secondMetadata.Select(m => m.TransactionPartitionIndex));
    }

    [Fact]
    public async Task Should_Append_Nothing_When_Any_Stream_Conflicts()
    {
        var first = NewGrainId();
        var second = NewGrainId();
        await Storage.AppendAsync(GrainTypeName, second, CreateEvents(2), 0, Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<LogAppendConflictException>(() =>
            AppendGroupedAsync([Append(first, CreateEvents(1), 0), Append(second, CreateEvents(1), 1)], Guid.NewGuid()));

        var conflict = Assert.Single(ex.Conflicts);
        Assert.Equal(second, conflict.GrainId);
        Assert.Equal(1, conflict.ExpectedVersion);
        Assert.Equal(2, conflict.ActualVersion);
        Assert.Equal(0, await Storage.GetLastVersionAsync(GrainTypeName, first));
        Assert.Equal(2, await Storage.GetLastVersionAsync(GrainTypeName, second));
    }

    [Fact]
    public async Task Should_Report_A_Missing_Stream_As_Version_Zero()
    {
        var first = NewGrainId();
        var missing = NewGrainId();

        var ex = await Assert.ThrowsAsync<LogAppendConflictException>(() =>
            AppendGroupedAsync([Append(first, CreateEvents(1), 0), Append(missing, CreateEvents(1), 2)], Guid.NewGuid()));

        var conflict = Assert.Single(ex.Conflicts);
        Assert.Equal(missing, conflict.GrainId);
        Assert.Equal(0, conflict.ActualVersion);
        Assert.Equal(0, await Storage.GetLastVersionAsync(GrainTypeName, first));
    }

    [Fact]
    public async Task Should_Check_A_Stream_Without_Appending_To_It()
    {
        var writer = NewGrainId();
        var checkedOnly = NewGrainId();
        await Storage.AppendAsync(GrainTypeName, checkedOnly, CreateEvents(2), 0, Guid.NewGuid());

        var versions = await AppendGroupedAsync([Append(writer, CreateEvents(1), 0), Append(checkedOnly, [], 2)], Guid.NewGuid());

        Assert.Equal([1, 2], versions);
        Assert.Equal(2, await Storage.GetLastVersionAsync(GrainTypeName, checkedOnly));

        await Assert.ThrowsAsync<LogAppendConflictException>(() =>
            AppendGroupedAsync([Append(writer, CreateEvents(1), 1), Append(checkedOnly, [], 1)], Guid.NewGuid()));
        Assert.Equal(1, await Storage.GetLastVersionAsync(GrainTypeName, writer));
    }

    [Fact]
    public async Task Should_Append_Without_A_Check_When_No_Expected_Version()
    {
        var first = NewGrainId();
        var second = NewGrainId();
        await Storage.AppendAsync(GrainTypeName, second, CreateEvents(2), 0, Guid.NewGuid());

        var versions = await AppendGroupedAsync([Append(first, CreateEvents(1), 0), Append(second, CreateEvents(1), null)], Guid.NewGuid());

        Assert.Equal([1, 3], versions);
    }

    [Fact]
    public async Task Should_Reject_Invalid_Appends()
    {
        var grainId = NewGrainId();

        await Assert.ThrowsAsync<ArgumentException>(() => Storage.AppendAsync(Array.Empty<LogStream>(), Array.Empty<LogRecord<SnackEvent>>(), Guid.NewGuid()));
        await Assert.ThrowsAsync<ArgumentException>(() => AppendGroupedAsync([Append(grainId, [], 0)], Guid.NewGuid()));
        await Assert.ThrowsAsync<ArgumentException>(() => AppendGroupedAsync([Append(grainId, CreateEvents(1), 0), Append(grainId, CreateEvents(1), 0)], Guid.NewGuid()));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => AppendGroupedAsync([Append(grainId, CreateEvents(1), -1)], Guid.NewGuid()));
    }

    private async Task<List<EventMetadata>> ReadMetadataAsync(GrainId grainId)
    {
        var client = SiloServices.GetRequiredKeyedService<KurrentDBClient>("journaledGrainLog");
        return await client.ReadStreamAsync(Direction.Forwards, $"{grainId.Type}-{grainId.Key}", StreamPosition.Start, cancellationToken: TestContext.Current.CancellationToken)
            .Select(e => KurrentDBEventMetadataCodec.Read(e.Event.Metadata.Span))
            .ToListAsync(TestContext.Current.CancellationToken);
    }
}
