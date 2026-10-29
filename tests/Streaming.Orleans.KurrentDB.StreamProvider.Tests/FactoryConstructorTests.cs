using System.Diagnostics.Metrics;

using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.StatisticMonitors;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that the cache and receiver factories reject missing dependencies when they are constructed rather than
///     failing later, when a queue is first created.
/// </summary>
public class FactoryConstructorTests
{
    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private static readonly KurrentDBQueueDataAdapter Adapter = new(OrleansSerializer, new KurrentDBDataAdapterOptions
    {
        ConnectionName = "test",
        ProviderName = "test",
        TypeMapper = new FakeTypeMapper(),
        GrainStorageSerializer = new FakeSerializer(),
    });

    private static readonly OrleansInstruments Instruments = new(new ServiceCollection().AddMetrics().BuildServiceProvider().GetRequiredService<IMeterFactory>());

    private static KurrentDBQueueCacheFactory CreateCacheFactory(
        KurrentDBStreamCachePressureOptions? pressureOptions = null,
        KurrentDBStreamCacheBufferPoolOptions? bufferPoolOptions = null,
        StreamCacheEvictionOptions? evictionOptions = null,
        StreamStatisticOptions? statisticOptions = null,
        IKurrentDBDataAdapter? dataAdapter = null,
        KurrentDBMonitorAggregationDimensions? sharedDimensions = null,
        OrleansInstruments? instruments = null,
        TimeProvider? timeProvider = null,
        bool nullPressure = false, bool nullBufferPool = false, bool nullEviction = false, bool nullStatistic = false,
        bool nullAdapter = false, bool nullDimensions = false, bool nullInstruments = false, bool nullTimeProvider = false) =>
        new(nullPressure ? null! : pressureOptions ?? new KurrentDBStreamCachePressureOptions(),
            nullBufferPool ? null! : bufferPoolOptions ?? new KurrentDBStreamCacheBufferPoolOptions(),
            nullEviction ? null! : evictionOptions ?? new StreamCacheEvictionOptions(),
            nullStatistic ? null! : statisticOptions ?? new StreamStatisticOptions(),
            nullAdapter ? null! : dataAdapter ?? Adapter,
            nullDimensions ? null! : sharedDimensions ?? new KurrentDBMonitorAggregationDimensions("test"),
            nullInstruments ? null! : instruments ?? Instruments,
            nullTimeProvider ? null! : timeProvider ?? TimeProvider.System);

    [Fact]
    public void The_Cache_Factory_Accepts_All_Dependencies()
    {
        Assert.NotNull(CreateCacheFactory());
    }

    [Theory]
    [InlineData("pressureOptions")]
    [InlineData("bufferPoolOptions")]
    [InlineData("evictionOptions")]
    [InlineData("statisticOptions")]
    [InlineData("dataAdater")]
    [InlineData("sharedDimensions")]
    [InlineData("instruments")]
    [InlineData("timeProvider")]
    public void The_Cache_Factory_Rejects_A_Missing_Dependency(string parameterName)
    {
        var exception = Assert.Throws<ArgumentNullException>(() => CreateCacheFactory(
            nullPressure: parameterName == "pressureOptions",
            nullBufferPool: parameterName == "bufferPoolOptions",
            nullEviction: parameterName == "evictionOptions",
            nullStatistic: parameterName == "statisticOptions",
            nullAdapter: parameterName == "dataAdater",
            nullDimensions: parameterName == "sharedDimensions",
            nullInstruments: parameterName == "instruments",
            nullTimeProvider: parameterName == "timeProvider"));

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Fact]
    public void The_Persistent_Subscription_Receiver_Factory_Rejects_A_Missing_Service_Provider()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new KurrentDBPersistentSubscriptionReceiverFactory(null!, "test"));

        Assert.Equal("serviceProvider", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void The_Persistent_Subscription_Receiver_Factory_Rejects_A_Missing_Name(string? name)
    {
        using var services = new ServiceCollection().BuildServiceProvider();

        var exception = Assert.ThrowsAny<ArgumentException>(() => new KurrentDBPersistentSubscriptionReceiverFactory(services, name!));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void The_All_Stream_Receiver_Factory_Rejects_A_Missing_Service_Provider()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new KurrentDBAllStreamReceiverFactory(null!, "test"));

        Assert.Equal("serviceProvider", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void The_All_Stream_Receiver_Factory_Rejects_A_Missing_Name(string? name)
    {
        using var services = new ServiceCollection().BuildServiceProvider();

        var exception = Assert.ThrowsAny<ArgumentException>(() => new KurrentDBAllStreamReceiverFactory(services, name!));

        Assert.Equal("name", exception.ParamName);
    }
}
