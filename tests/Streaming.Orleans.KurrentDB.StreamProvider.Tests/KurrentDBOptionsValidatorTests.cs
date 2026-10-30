using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that each stream provider options validator accepts a complete configuration and rejects every missing
///     or out-of-range setting with an error that names the provider.
/// </summary>
public class KurrentDBOptionsValidatorTests
{
    private const string ProviderName = "KurrentDBProvider";

    private sealed class TestTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private sealed class FakeCheckpointStore : ICheckpointStore<ulong>
    {
        public Task<ulong> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default) => Task.FromResult(0UL);

        public Task StoreCheckpointAsync(string subscriptionName, string partitionId, ulong streamPosition, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class TestOperationOptions : KurrentDBOperationOptions;

    private static void AssertInvalid(IConfigurationValidator validator, string expectedSetting)
    {
        var exception = Assert.Throws<OrleansConfigurationException>(validator.ValidateConfiguration);
        Assert.Contains(ProviderName, exception.Message);
        Assert.Contains(expectedSetting, exception.Message);
    }

    #region KurrentDBOptions

    private static KurrentDBOptions ValidKurrentDBOptions() => new()
    {
        ConnectionName = "kurrentdb",
        Name = ProviderName,
        Queues = ["queue-0"]
    };

    [Fact]
    public void KurrentDBOptions_Accepts_a_complete_configuration()
    {
        new KurrentDBOptionsValidator(ValidKurrentDBOptions(), ProviderName).ValidateConfiguration();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void KurrentDBOptions_Rejects_a_missing_connection_name(string? connectionName)
    {
        var options = ValidKurrentDBOptions();
        options.ConnectionName = connectionName!;

        AssertInvalid(new KurrentDBOptionsValidator(options, ProviderName), nameof(KurrentDBOptions.ConnectionName));
    }

    [Fact]
    public void KurrentDBOptions_Rejects_missing_credentials()
    {
        var options = ValidKurrentDBOptions();
        options.Credentials = null!;

        AssertInvalid(new KurrentDBOptionsValidator(options, ProviderName), nameof(KurrentDBOptions.Credentials));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void KurrentDBOptions_Rejects_a_missing_name(string? name)
    {
        var options = ValidKurrentDBOptions();
        options.Name = name!;

        AssertInvalid(new KurrentDBOptionsValidator(options, ProviderName), nameof(KurrentDBOptions.Name));
    }

    [Fact]
    public void KurrentDBOptions_Rejects_null_queues()
    {
        var options = ValidKurrentDBOptions();
        options.Queues = null!;

        AssertInvalid(new KurrentDBOptionsValidator(options, ProviderName), nameof(KurrentDBOptions.Queues));
    }

    [Fact]
    public void KurrentDBOptions_Rejects_empty_queues()
    {
        var options = ValidKurrentDBOptions();
        options.Queues = [];

        AssertInvalid(new KurrentDBOptionsValidator(options, ProviderName), nameof(KurrentDBOptions.Queues));
    }

    #endregion

    #region KurrentDBDataAdapterOptions

    private static KurrentDBDataAdapterOptions ValidDataAdapterOptions() => new()
    {
        ConnectionName = "kurrentdb",
        ProviderName = ProviderName,
        TypeMapper = new TestTypeMapper(),
        GrainStorageSerializer = new FakeSerializer()
    };

    [Fact]
    public void DataAdapterOptions_Accepts_a_complete_configuration()
    {
        new KurrentDBDataAdapterOptionsValidator(ValidDataAdapterOptions(), ProviderName).ValidateConfiguration();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void DataAdapterOptions_Rejects_a_missing_connection_name(string? connectionName)
    {
        var options = ValidDataAdapterOptions();
        options.ConnectionName = connectionName!;

        AssertInvalid(new KurrentDBDataAdapterOptionsValidator(options, ProviderName), nameof(KurrentDBDataAdapterOptions.ConnectionName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void DataAdapterOptions_Rejects_a_missing_provider_name(string? providerName)
    {
        var options = ValidDataAdapterOptions();
        options.ProviderName = providerName!;

        AssertInvalid(new KurrentDBDataAdapterOptionsValidator(options, ProviderName), nameof(KurrentDBDataAdapterOptions.ProviderName));
    }

    [Fact]
    public void DataAdapterOptions_Rejects_a_provider_name_containing_the_namespace_separator()
    {
        var options = ValidDataAdapterOptions();
        options.ProviderName = $"bad{KurrentDBStreamNamespace.Separator}name";

        AssertInvalid(new KurrentDBDataAdapterOptionsValidator(options, ProviderName), nameof(KurrentDBDataAdapterOptions.ProviderName));
    }

    [Fact]
    public void DataAdapterOptions_Rejects_a_missing_stream_id_mapper()
    {
        var options = ValidDataAdapterOptions();
        options.StreamIdMapper = null!;

        AssertInvalid(new KurrentDBDataAdapterOptionsValidator(options, ProviderName), nameof(KurrentDBDataAdapterOptions.StreamIdMapper));
    }

    [Fact]
    public void DataAdapterOptions_Rejects_a_missing_serializer()
    {
        var options = ValidDataAdapterOptions();
        options.GrainStorageSerializer = null!;

        AssertInvalid(new KurrentDBDataAdapterOptionsValidator(options, ProviderName), nameof(KurrentDBDataAdapterOptions.GrainStorageSerializer));
    }

    [Fact]
    public void DataAdapterOptions_Rejects_a_missing_type_mapper()
    {
        var options = ValidDataAdapterOptions();
        options.TypeMapper = null!;

        AssertInvalid(new KurrentDBDataAdapterOptionsValidator(options, ProviderName), nameof(KurrentDBDataAdapterOptions.TypeMapper));
    }

    #endregion

    #region KurrentDBOperationOptions

    [Fact]
    public void OperationOptions_Accepts_a_complete_configuration()
    {
        new KurrentDBOperationOptionsValidator<TestOperationOptions>(new TestOperationOptions { ConnectionName = "kurrentdb" }, ProviderName).ValidateConfiguration();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void OperationOptions_Rejects_a_missing_connection_name(string? connectionName)
    {
        var options = new TestOperationOptions { ConnectionName = connectionName };

        AssertInvalid(new KurrentDBOperationOptionsValidator<TestOperationOptions>(options, ProviderName), nameof(KurrentDBOperationOptions.ConnectionName));
    }

    [Fact]
    public void OperationOptions_Rejects_missing_credentials()
    {
        var options = new TestOperationOptions { ConnectionName = "kurrentdb", Credentials = null! };

        AssertInvalid(new KurrentDBOperationOptionsValidator<TestOperationOptions>(options, ProviderName), nameof(KurrentDBOperationOptions.Credentials));
    }

    #endregion

    #region KurrentDBPersistentSubscriptionReceiverOptions

    [Fact]
    public void PersistentSubscriptionOptions_Accepts_a_complete_configuration()
    {
        new KurrentDBPersistentSubscriptionReceiverOptionsValidator(new KurrentDBPersistentSubscriptionReceiverOptions(), ProviderName).ValidateConfiguration();
    }

    [Fact]
    public void PersistentSubscriptionOptions_Rejects_missing_subscription_settings()
    {
        var options = new KurrentDBPersistentSubscriptionReceiverOptions { SubscriptionSettings = null! };

        AssertInvalid(new KurrentDBPersistentSubscriptionReceiverOptionsValidator(options, ProviderName), nameof(KurrentDBPersistentSubscriptionReceiverOptions.SubscriptionSettings));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PersistentSubscriptionOptions_Rejects_a_non_positive_prefetch_count(int prefetchCount)
    {
        var options = new KurrentDBPersistentSubscriptionReceiverOptions { PrefetchCount = prefetchCount };

        AssertInvalid(new KurrentDBPersistentSubscriptionReceiverOptionsValidator(options, ProviderName), nameof(KurrentDBPersistentSubscriptionReceiverOptions.PrefetchCount));
    }

    #endregion

    #region KurrentDBCatchUpSubscriptionReceiverOptions

    private static KurrentDBCatchUpSubscriptionReceiverOptions ValidCatchUpOptions() => new()
    {
        CheckpointStore = new FakeCheckpointStore()
    };

    [Fact]
    public void CatchUpOptions_Accepts_a_complete_configuration()
    {
        new KurrentDBCatchUpSubscriptionReceiverOptionsValidator(ValidCatchUpOptions(), ValidKurrentDBOptions(), ProviderName).ValidateConfiguration();
    }

    [Fact]
    public void CatchUpOptions_Rejects_null_queues()
    {
        var kurrentDBOptions = ValidKurrentDBOptions();
        kurrentDBOptions.Queues = null!;

        AssertInvalid(new KurrentDBCatchUpSubscriptionReceiverOptionsValidator(ValidCatchUpOptions(), kurrentDBOptions, ProviderName), nameof(KurrentDBOptions.Queues));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void CatchUpOptions_Requires_exactly_one_queue(int queueCount)
    {
        var kurrentDBOptions = ValidKurrentDBOptions();
        kurrentDBOptions.Queues = Enumerable.Range(0, queueCount).Select(i => $"queue-{i}").ToList();

        AssertInvalid(new KurrentDBCatchUpSubscriptionReceiverOptionsValidator(ValidCatchUpOptions(), kurrentDBOptions, ProviderName), nameof(KurrentDBOptions.Queues));
    }

    [Fact]
    public void CatchUpOptions_Rejects_a_missing_checkpoint_store()
    {
        var options = ValidCatchUpOptions();
        options.CheckpointStore = null!;

        AssertInvalid(new KurrentDBCatchUpSubscriptionReceiverOptionsValidator(options, ValidKurrentDBOptions(), ProviderName), nameof(KurrentDBCatchUpSubscriptionReceiverOptions.CheckpointStore));
    }

    [Fact]
    public void CatchUpOptions_Rejects_missing_checkpoint_monitor_options()
    {
        var options = ValidCatchUpOptions();
        options.CheckpointMonitorOptions = null!;

        AssertInvalid(new KurrentDBCatchUpSubscriptionReceiverOptionsValidator(options, ValidKurrentDBOptions(), ProviderName), nameof(KurrentDBCatchUpSubscriptionReceiverOptions.CheckpointMonitorOptions));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue / KurrentDBCatchUpQueueAdapterReceiver.MaxMessagesPerRead + 1)]
    public void CatchUpOptions_Rejects_out_of_range_buffer_capacity_multiplier(int bufferCapacityMultiplier)
    {
        var options = ValidCatchUpOptions();
        options.BufferCapacityMultiplier = bufferCapacityMultiplier;

        AssertInvalid(new KurrentDBCatchUpSubscriptionReceiverOptionsValidator(options, ValidKurrentDBOptions(), ProviderName), nameof(KurrentDBCatchUpSubscriptionReceiverOptions.BufferCapacityMultiplier));
    }

    #endregion

    #region StreamPullingAgentOptions

    [Fact]
    public void PullingAgentOptions_Accepts_a_batch_size_of_one()
    {
        new KurrentDBCatchUpPullingAgentOptionsValidator(new StreamPullingAgentOptions { BatchContainerBatchSize = 1 }, ProviderName).ValidateConfiguration();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(100)]
    public void PullingAgentOptions_Rejects_a_batch_size_above_one(int batchSize)
    {
        var options = new StreamPullingAgentOptions { BatchContainerBatchSize = batchSize };

        AssertInvalid(new KurrentDBCatchUpPullingAgentOptionsValidator(options, ProviderName), nameof(StreamPullingAgentOptions.BatchContainerBatchSize));
    }

    #endregion

    #region KurrentDBStreamCacheBufferPoolOptions

    [Fact]
    public void BufferPoolOptions_Accepts_the_defaults()
    {
        new KurrentDBStreamCacheBufferPoolOptionsValidator(new KurrentDBStreamCacheBufferPoolOptions(), ProviderName).ValidateConfiguration();
    }

    [Fact]
    public void BufferPoolOptions_Accepts_a_zero_or_infinite_trim_window()
    {
        new KurrentDBStreamCacheBufferPoolOptionsValidator(new KurrentDBStreamCacheBufferPoolOptions { TrimWindow = TimeSpan.Zero }, ProviderName).ValidateConfiguration();
        new KurrentDBStreamCacheBufferPoolOptionsValidator(new KurrentDBStreamCacheBufferPoolOptions { TrimWindow = Timeout.InfiniteTimeSpan }, ProviderName).ValidateConfiguration();
    }

    [Fact]
    public void BufferPoolOptions_Rejects_a_negative_trim_window()
    {
        var options = new KurrentDBStreamCacheBufferPoolOptions { TrimWindow = TimeSpan.FromSeconds(-5) };

        AssertInvalid(new KurrentDBStreamCacheBufferPoolOptionsValidator(options, ProviderName), nameof(KurrentDBStreamCacheBufferPoolOptions.TrimWindow));
    }

    [Fact]
    public void BufferPoolOptions_Rejects_negative_extra_blocks()
    {
        var options = new KurrentDBStreamCacheBufferPoolOptions { ExtraBlocks = -1 };

        AssertInvalid(new KurrentDBStreamCacheBufferPoolOptionsValidator(options, ProviderName), nameof(KurrentDBStreamCacheBufferPoolOptions.ExtraBlocks));
    }

    [Fact]
    public void BufferPoolOptions_Rejects_negative_headroom()
    {
        var options = new KurrentDBStreamCacheBufferPoolOptions { Headroom = -1 };

        AssertInvalid(new KurrentDBStreamCacheBufferPoolOptionsValidator(options, ProviderName), nameof(KurrentDBStreamCacheBufferPoolOptions.Headroom));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BufferPoolOptions_Rejects_fewer_than_one_consecutive_window(int consecutiveWindows)
    {
        var options = new KurrentDBStreamCacheBufferPoolOptions { ConsecutiveWindows = consecutiveWindows };

        AssertInvalid(new KurrentDBStreamCacheBufferPoolOptionsValidator(options, ProviderName), nameof(KurrentDBStreamCacheBufferPoolOptions.ConsecutiveWindows));
    }

    #endregion
}
