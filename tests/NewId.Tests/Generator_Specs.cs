// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

using System.Diagnostics;

namespace Continuum.Tests.NewIdGeneratorTests;

public class When_generating_id
{
    [Fact]
    public void Should_match_when_all_providers_equal()
    {
        // Arrange
        var generator1 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);
        var generator2 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);

        // Act
        var id1 = generator1.Next();
        var id2 = generator2.Next();

        // Assert
        Assert.Equal(id2, id1);
    }

    [Fact]
    public void Should_match_when_all_providers_equal_with_guid_method()
    {
        // Arrange
        var generator1 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);
        var generator2 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);
        generator1.Next().ToSqlServerGuid();
        generator2.NextSqlServerGuid();

        // Act
        var id1 = generator1.Next().ToSqlServerGuid();
        var id2 = generator2.NextSqlServerGuid();

        // Assert
        Assert.Equal(id2, id1);
    }

    [Fact]
    public void Should_not_match_when_generated_from_two_processes()
    {
        // Arrange
        var generator1 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);

        _processIdProvider = new MockProcessIdProvider(BitConverter.GetBytes(11));
        var generator2 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);

        // Act
        var id1 = generator1.Next();
        var id2 = generator2.Next();

        // Assert
        Assert.NotEqual(id2, id1);
    }

    [Fact]
    public void Should_not_match_when_processor_id_provided()
    {
        // Arrange
        var generator1 = new NewIdGenerator(_tickProvider, _workerIdProvider);
        var generator2 = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);

        // Act
        var id1 = generator1.Next();
        var id2 = generator2.Next();

        // Assert
        Assert.NotEqual(id2, id1);
    }

    [Fact]
    public void Should_match_sequentially()
    {
        var generator = new NewIdGenerator(_tickProvider, _workerIdProvider);

        var id1 = generator.Next().ToSqlServerGuid();
        var id2 = generator.NextSqlServerGuid();
        var id3 = generator.NextSqlServerGuid();

        Assert.NotEqual(id2, id1);
        Assert.NotEqual(id3, id2);
        Assert.True(id2.CompareTo(id1) > 0);

        Console.WriteLine(id1);
        Console.WriteLine(id2);
        Console.WriteLine(id3);

        NewId nid1 = id1.ToNewIdFromSqlServer();
        NewId nid2 = id2.ToNewIdFromSqlServer();
    }

    [Fact]
    public void Should_match_sequentially_with_sequential_guid()
    {
        var generator = new NewIdGenerator(_tickProvider, _workerIdProvider);

        var nid = generator.Next();
        var id1 = nid.ToSequentialGuid();
        var id2 = generator.NextSequentialGuid();
        var id3 = generator.NextSequentialGuid();

        Assert.NotEqual(id2, id1);
        Assert.NotEqual(id3, id2);
        Assert.True(id2.CompareTo(id1) > 0);

        Console.WriteLine(id1);
        Console.WriteLine(id2);
        Console.WriteLine(id3);

        NewId nid1 = id1.ToNewIdFromSequential();
        NewId nid2 = id2.ToNewIdFromSequential();

        Assert.Equal(nid1, nid);
    }

    [Fact]
    public void Should_generate_known_sequential_guid()
    {
        var expected = Guid.Parse("ffbbf0a1-ae2b-8a1d-bd81-bf34437f0b01");
        var tickProvider = new MockTickProvider(new DateTime(2882, 6, 1, 12, 34, 56, DateTimeKind.Utc).AddTicks(1234567).Ticks);
        var networkProvider = new MockNetworkProvider(BitConverter.GetBytes(6857996259202924925));
        var generator = new NewIdGenerator(tickProvider, networkProvider);

        for (int i = 0; i < 267; i++)
        {
            generator.NextSequentialGuid();
        }
        var guid = generator.NextSequentialGuid();

        Assert.Equal(guid, expected);
    }

    [Fact]
    public void Should_generate_known_guid()
    {
        var expected = Guid.Parse("437f0b01-bf34-bd81-8a1d-ffbbf0a1ae2b");
        var tickProvider = new MockTickProvider(new DateTime(2882, 6, 1, 12, 34, 56, DateTimeKind.Utc).AddTicks(1234567).Ticks);
        var networkProvider = new MockNetworkProvider(BitConverter.GetBytes(6857996259202924925));
        var generator = new NewIdGenerator(tickProvider, networkProvider);

        for (int i = 0; i < 267; i++)
        {
            generator.NextSqlServerGuid();
        }
        var guid = generator.NextSqlServerGuid();

        Assert.Equal(guid, expected);
    }

    [Fact]
    public void Should_generate_uuid_version_8_with_rfc_variant()
    {
        var generator = new NewIdGenerator(_tickProvider, _workerIdProvider, _processIdProvider);

        var guid = generator.NextSequentialGuid();

        Assert.Equal(8, guid.Version);
        Assert.Equal(0b1000, guid.Variant & 0b1100);
        Assert.Equal(8, generator.Next().ToSequentialGuid().Version);
    }

    [Theory]
    [InlineData(621355968000000000L)]
    [InlineData(638843228961234567L)]
    [InlineData(621355968000000000L + (1L << 58) - 1)]
    public void Should_round_trip_timestamp(long ticks)
    {
        var generator = new NewIdGenerator(new MockTickProvider(ticks), _workerIdProvider);

        var id = generator.Next();

        Assert.Equal(ticks, id.Timestamp.Ticks);
        Assert.Equal(DateTimeKind.Utc, id.Timestamp.Kind);
    }

    [Theory]
    [InlineData(621355967999999999L)]
    [InlineData(621355968000000000L + (1L << 58))]
    public void Should_reject_ticks_outside_supported_range(long ticks)
    {
        var generator = new NewIdGenerator(new MockTickProvider(ticks), _workerIdProvider);

        Assert.Throws<InvalidOperationException>(() => generator.Next());
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, -1)]
    [InlineData(1, int.MaxValue)]
    public async Task Should_reject_invalid_range_without_blocking_later_calls(int index, int count)
    {
        var generator = new NewIdGenerator(_tickProvider, _workerIdProvider);
        var cancellationToken = TestContext.Current.CancellationToken;

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Next(new NewId[4], index, count));

        // Times out if the failed call left the generator lock held.
        await Task.Run(generator.Next, cancellationToken).WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
    }

    public When_generating_id()
    {
        _start = DateTime.UtcNow;
        _stopwatch = Stopwatch.StartNew();

        _tickProvider = new MockTickProvider(GetTicks());
        _workerIdProvider = new MockNetworkProvider(BitConverter.GetBytes(1234567890L));
        _processIdProvider = new MockProcessIdProvider(BitConverter.GetBytes(10));
    }

    ITickProvider _tickProvider;
    IWorkerIdProvider _workerIdProvider;
    IProcessIdProvider _processIdProvider;
    DateTime _start;
    Stopwatch _stopwatch;

    long GetTicks()
    {
        return _start.AddTicks(_stopwatch.Elapsed.Ticks).Ticks;
    }


    class MockTickProvider :
        ITickProvider
    {
        public MockTickProvider(long ticks)
        {
            Ticks = ticks;
        }

        public long Ticks { get; }
    }


    class MockNetworkProvider :
        IWorkerIdProvider
    {
        readonly byte[] _workerId;

        public MockNetworkProvider(byte[] workerId)
        {
            _workerId = workerId;
        }

        public byte[] GetWorkerId(int index)
        {
            return _workerId;
        }
    }

    class MockProcessIdProvider :
        IProcessIdProvider
    {
        readonly byte[] _processId;

        public MockProcessIdProvider(byte[] processId)
        {
            _processId = processId;
        }

        public byte[] GetProcessId()
        {
            return _processId;
        }
    }
}
