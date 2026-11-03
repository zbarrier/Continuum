using Continuum.Streaming.Orleans.KurrentDB.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Configuration;
using Continuum.TypeMapping;

using Orleans.Runtime;
using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Tests;

/// <summary>
/// Verifies that the catch-up subscription options validator accepts a complete configuration and rejects each
/// missing or out-of-range setting with an error that names the subscription and the setting.
/// </summary>
public class KurrentDBCatchupSubscriptionOptionsValidatorTests
{
    private const string SubscriptionName = "Orders";

    private sealed class TestTypeMapper : TypeMapper;

    private sealed class FakeCheckpointStore : ICheckpointStore<ulong>
    {
        public Task<ulong?> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default) => Task.FromResult<ulong?>(null);

        public Task StoreCheckpointAsync(string subscriptionName, string partitionId, ulong streamPosition, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private static KurrentDBCatchupSubscriptionOptions ValidOptions() => new()
    {
        ConnectionName = "kurrentdb",
        CheckpointStore = new FakeCheckpointStore(),
        GrainStorageSerializer = new FakeSerializer(),
        TypeMapper = new TestTypeMapper(),
        StreamNameParser = new DefaultKurrentDBStreamNameParser(),
    };

    private static void Validate(KurrentDBCatchupSubscriptionOptions options)
    {
        new KurrentDBCatchupSubscriptionOptionsValidator(options, new KurrentDBCatchupSubscriptionInfo(SubscriptionName, typeof(object))).ValidateConfiguration();
    }

    private static void AssertRejected(KurrentDBCatchupSubscriptionOptions options, string setting)
    {
        var exception = Assert.Throws<OrleansConfigurationException>(() => Validate(options));
        Assert.Contains(SubscriptionName, exception.Message);
        Assert.Contains(setting, exception.Message);
    }

    [Fact]
    public void Accepts_a_complete_configuration()
    {
        Validate(ValidOptions());
    }

    [Fact]
    public void Rejects_null_options()
    {
        Assert.Throws<OrleansConfigurationException>(() => new KurrentDBCatchupSubscriptionOptionsValidator(null!, new KurrentDBCatchupSubscriptionInfo(SubscriptionName, typeof(object))));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_a_missing_connection_name(string? connectionName)
    {
        var options = ValidOptions();
        options.ConnectionName = connectionName!;

        AssertRejected(options, nameof(KurrentDBCatchupSubscriptionOptions.ConnectionName));
    }

    [Fact]
    public void Rejects_missing_buffer_options()
    {
        var options = ValidOptions();
        options.BufferOptions = null!;

        AssertRejected(options, nameof(KurrentDBCatchupSubscriptionOptions.BufferOptions));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100_001)]
    public void Rejects_max_events_per_process_loop_out_of_range(int value)
    {
        var options = ValidOptions();
        options.BufferOptions.MaxEventsPerProcessLoop = value;

        AssertRejected(options, nameof(KurrentDBChannelBufferOptions.MaxEventsPerProcessLoop));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1_000_001)]
    public void Rejects_max_unprocessed_events_before_stop_out_of_range(int value)
    {
        var options = ValidOptions();
        options.BufferOptions.MaxUnprocessedEventsBeforeStop = value;

        AssertRejected(options, nameof(KurrentDBChannelBufferOptions.MaxUnprocessedEventsBeforeStop));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(100_000, 1_000_000)]
    public void Accepts_buffer_options_at_the_boundaries(int maxEventsPerProcessLoop, int maxUnprocessedEventsBeforeStop)
    {
        var options = ValidOptions();
        options.BufferOptions.MaxEventsPerProcessLoop = maxEventsPerProcessLoop;
        options.BufferOptions.MaxUnprocessedEventsBeforeStop = maxUnprocessedEventsBeforeStop;

        Validate(options);
    }

    [Fact]
    public void Rejects_missing_checkpoint_monitor_options()
    {
        var options = ValidOptions();
        options.CheckpointMonitorOptions = null!;

        AssertRejected(options, nameof(KurrentDBCatchupSubscriptionOptions.CheckpointMonitorOptions));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1_000_001)]
    public void Rejects_checkpoint_interval_out_of_range(int value)
    {
        var options = ValidOptions();
        options.CheckpointMonitorOptions.KurrentDBCheckpointInterval = (uint)value;

        AssertRejected(options, "KurrentDBCheckpointInterval");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1_000_001)]
    public void Rejects_max_events_before_commit_out_of_range(int value)
    {
        var options = ValidOptions();
        options.CheckpointMonitorOptions.MaxEventsBeforeCommit = (ulong)value;

        AssertRejected(options, "MaxEventsBeforeCommit");
    }

    [Theory]
    [InlineData("00:00:00.999")]
    [InlineData("1.00:00:00.001")]
    public void Rejects_max_time_before_commit_out_of_range(string value)
    {
        var options = ValidOptions();
        options.CheckpointMonitorOptions.MaxTimeBeforeCommit = TimeSpan.Parse(value);

        AssertRejected(options, "MaxTimeBeforeCommit");
    }

    [Fact]
    public void Rejects_missing_credentials()
    {
        var options = ValidOptions();
        options.Credentials = null!;

        AssertRejected(options, nameof(KurrentDBCatchupSubscriptionOptions.Credentials));
    }

    [Fact]
    public void Rejects_missing_retry_options()
    {
        var options = ValidOptions();
        options.RetryOptions = null!;

        AssertRejected(options, nameof(KurrentDBCatchupSubscriptionOptions.RetryOptions));
    }

    [Theory]
    [InlineData("00:00:00.009")]
    [InlineData("00:01:00.001")]
    public void Rejects_retry_base_delay_out_of_range(string value)
    {
        var options = ValidOptions();
        options.RetryOptions.BaseDelay = TimeSpan.Parse(value);

        AssertRejected(options, nameof(KurrentDBBatchRetryOptions.BaseDelay));
    }

    [Theory]
    [InlineData("00:00:00.500")]
    [InlineData("01:00:00.001")]
    public void Rejects_retry_max_delay_below_base_delay_or_above_one_hour(string value)
    {
        var options = ValidOptions();
        options.RetryOptions.MaxDelay = TimeSpan.Parse(value);

        AssertRejected(options, nameof(KurrentDBBatchRetryOptions.MaxDelay));
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(null, "admin", null)]
    [InlineData(null, null, "changeit")]
    public void Rejects_explicit_credentials_without_a_token_or_username_and_password(string? authToken, string? username, string? password)
    {
        var options = ValidOptions();
        options.Credentials = new KurrentDBSubscriptionCredentialsOptions
        {
            UseDefault = false,
            AuthToken = authToken!,
            Username = username!,
            Password = password!,
        };

        AssertRejected(options, nameof(KurrentDBCatchupSubscriptionOptions.Credentials));
    }

    [Theory]
    [InlineData("token", null, null)]
    [InlineData(null, "admin", "changeit")]
    public void Accepts_explicit_credentials_with_a_token_or_username_and_password(string? authToken, string? username, string? password)
    {
        var options = ValidOptions();
        options.Credentials = new KurrentDBSubscriptionCredentialsOptions
        {
            UseDefault = false,
            AuthToken = authToken!,
            Username = username!,
            Password = password!,
        };

        Validate(options);
    }

    [Fact]
    public void Rejects_a_missing_checkpoint_store()
    {
        var options = ValidOptions();
        options.CheckpointStore = null!;

        AssertRejected(options, nameof(KurrentDBCatchupSubscriptionOptions.CheckpointStore));
    }

    [Fact]
    public void Rejects_a_missing_grain_storage_serializer()
    {
        var options = ValidOptions();
        options.GrainStorageSerializer = null!;

        AssertRejected(options, nameof(KurrentDBCatchupSubscriptionOptions.GrainStorageSerializer));
    }

    [Fact]
    public void Rejects_a_missing_type_mapper()
    {
        var options = ValidOptions();
        options.TypeMapper = null!;

        AssertRejected(options, nameof(KurrentDBCatchupSubscriptionOptions.TypeMapper));
    }

    [Fact]
    public void Rejects_a_missing_stream_name_parser()
    {
        var options = ValidOptions();
        options.StreamNameParser = null!;

        AssertRejected(options, nameof(KurrentDBCatchupSubscriptionOptions.StreamNameParser));
    }
}
