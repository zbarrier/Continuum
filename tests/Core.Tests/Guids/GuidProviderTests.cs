using Continuum.Guids;

namespace Continuum.Tests.Guids;

public class GuidProviderTests
{
    [Fact]
    public void Version4_creates_version_4_without_timestamp()
    {
        var provider = new Version4GuidProvider();

        var id = provider.NewGuid();

        Assert.Equal(4, id.Version);
        Assert.False(provider.TryGetTimestamp(id, out _));
    }

    [Fact]
    public void Version7_round_trips_timestamp_to_the_millisecond()
    {
        var now = new DateTimeOffset(2025, 6, 1, 12, 34, 56, 789, TimeSpan.Zero);
        var provider = new Version7GuidProvider(new FixedTimeProvider(now));

        var id = provider.NewGuid();

        Assert.Equal(7, id.Version);
        Assert.True(provider.TryGetTimestamp(id, out var timestamp));
        Assert.Equal(now, timestamp);
    }

    [Fact]
    public void NewId_round_trips_timestamp()
    {
        var provider = new NewIdGuidProvider();
        var before = DateTimeOffset.UtcNow;

        var id = provider.NewGuid();

        Assert.Equal(8, id.Version);
        Assert.True(provider.TryGetTimestamp(id, out var timestamp));
        Assert.Equal(NewId.FromSequentialGuid(id).Timestamp, timestamp);
        Assert.InRange(timestamp, before.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void NewIdGuids_fills_destination_in_order()
    {
        var ids = new Guid[100];

        new NewIdGuidProvider().NewGuids(ids);

        Assert.All(ids, id => Assert.Equal(8, id.Version));
        Assert.Equal(ids.Length, ids.Distinct().Count());
        Assert.Equal(ids.Order(), ids);
    }

    [Fact]
    public void Default_NewGuids_fills_destination()
    {
        var ids = new Guid[10];

        new Version4GuidProvider().NewGuids(ids);

        Assert.All(ids, id => Assert.Equal(4, id.Version));
        Assert.Equal(ids.Length, ids.Distinct().Count());
    }

    [Fact]
    public void Providers_reject_other_formats()
    {
        var v4 = Guid.NewGuid();
        var v7 = Guid.CreateVersion7();
        var v8 = NewId.NextSequentialGuid();

        Assert.False(new Version7GuidProvider().TryGetTimestamp(v4, out _));
        Assert.False(new Version7GuidProvider().TryGetTimestamp(v8, out _));
        Assert.False(new NewIdGuidProvider().TryGetTimestamp(v4, out _));
        Assert.False(new NewIdGuidProvider().TryGetTimestamp(v7, out _));
    }

    sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
