using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Configuration;
using Continuum.Streams;
using Continuum.TypeMapping;

using Orleans.Runtime;
using Orleans.Storage;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Tests;

/// <summary>
/// Verifies that the catch-up subscription options validator accepts a complete configuration and rejects each
/// missing or out-of-range setting with an error that names the subscription and the setting.
/// </summary>
public class CosmosDBCatchupSubscriptionOptionsValidatorTests
{
    private const string SubscriptionName = "Orders";

    private sealed class TestTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private sealed class FakeStreamNameParser : IStreamNameParser
    {
        public IStreamName Parse(string streamName) => throw new NotSupportedException();
    }

    private static CosmosDBCatchupSubscriptionOptions ValidOptions() => new()
    {
        ConnectionName = "cosmos",
        DatabaseName = "events",
        LeaseContainerName = "leases",
        MonitoredContainerName = "streams",
        InstanceName = "worker-1",
        GrainStorageSerializer = new FakeSerializer(),
        StreamNameParser = new FakeStreamNameParser(),
        TypeMapper = new TestTypeMapper(),
    };

    private static void Validate(CosmosDBCatchupSubscriptionOptions options)
    {
        new CosmosDBCatchupSubscriptionOptionsValidator(options, new CosmosDBCatchupSubscriptionInfo(SubscriptionName, typeof(object))).ValidateConfiguration();
    }

    private static void AssertRejected(CosmosDBCatchupSubscriptionOptions options, string setting)
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
        Assert.Throws<OrleansConfigurationException>(() => new CosmosDBCatchupSubscriptionOptionsValidator(null!, new CosmosDBCatchupSubscriptionInfo(SubscriptionName, typeof(object))));
    }

    public static TheoryData<string, string?> MissingNames()
    {
        var data = new TheoryData<string, string?>();
        foreach (var setting in new[]
        {
            nameof(CosmosDBCatchupSubscriptionOptions.ConnectionName),
            nameof(CosmosDBCatchupSubscriptionOptions.DatabaseName),
            nameof(CosmosDBCatchupSubscriptionOptions.LeaseContainerName),
            nameof(CosmosDBCatchupSubscriptionOptions.MonitoredContainerName),
            nameof(CosmosDBCatchupSubscriptionOptions.InstanceName),
        })
        {
            data.Add(setting, null);
            data.Add(setting, "");
            data.Add(setting, "   ");
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(MissingNames))]
    public void Rejects_a_missing_name(string setting, string? value)
    {
        var options = ValidOptions();
        switch (setting)
        {
            case nameof(CosmosDBCatchupSubscriptionOptions.ConnectionName): options.ConnectionName = value!; break;
            case nameof(CosmosDBCatchupSubscriptionOptions.DatabaseName): options.DatabaseName = value!; break;
            case nameof(CosmosDBCatchupSubscriptionOptions.LeaseContainerName): options.LeaseContainerName = value!; break;
            case nameof(CosmosDBCatchupSubscriptionOptions.MonitoredContainerName): options.MonitoredContainerName = value!; break;
            case nameof(CosmosDBCatchupSubscriptionOptions.InstanceName): options.InstanceName = value!; break;
        }

        AssertRejected(options, setting);
    }

    [Fact]
    public void Rejects_a_from_date_time_start_without_a_date_time()
    {
        var options = ValidOptions();
        options.StartingPositionType = StartingPositionType.FromDateTime;

        AssertRejected(options, nameof(CosmosDBCatchupSubscriptionOptions.StartingPositionDateTime));
    }

    [Fact]
    public void Accepts_a_from_date_time_start_with_a_date_time()
    {
        var options = ValidOptions();
        options.StartingPositionType = StartingPositionType.FromDateTime;
        options.StartingPositionDateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Validate(options);
    }

    [Theory]
    [InlineData(StartingPositionType.BeginningOfTime)]
    [InlineData(StartingPositionType.Latest)]
    public void Accepts_other_starts_without_a_date_time(StartingPositionType startingPositionType)
    {
        var options = ValidOptions();
        options.StartingPositionType = startingPositionType;

        Validate(options);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(60_001)]
    public void Rejects_a_polling_interval_out_of_range(int value)
    {
        var options = ValidOptions();
        options.PollingIntervalInMilliseconds = value;

        AssertRejected(options, nameof(CosmosDBCatchupSubscriptionOptions.PollingIntervalInMilliseconds));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10_001)]
    public void Rejects_max_items_per_poll_out_of_range(int value)
    {
        var options = ValidOptions();
        options.MaxItemsPerPoll = value;

        AssertRejected(options, nameof(CosmosDBCatchupSubscriptionOptions.MaxItemsPerPoll));
    }

    [Theory]
    [InlineData(100, 1)]
    [InlineData(60_000, 10_000)]
    public void Accepts_polling_settings_at_the_boundaries(int pollingIntervalInMilliseconds, int maxItemsPerPoll)
    {
        var options = ValidOptions();
        options.PollingIntervalInMilliseconds = pollingIntervalInMilliseconds;
        options.MaxItemsPerPoll = maxItemsPerPoll;

        Validate(options);
    }

    [Fact]
    public void Rejects_a_missing_grain_storage_serializer()
    {
        var options = ValidOptions();
        options.GrainStorageSerializer = null!;

        AssertRejected(options, nameof(CosmosDBCatchupSubscriptionOptions.GrainStorageSerializer));
    }

    [Fact]
    public void Rejects_a_missing_stream_name_parser()
    {
        var options = ValidOptions();
        options.StreamNameParser = null!;

        AssertRejected(options, nameof(CosmosDBCatchupSubscriptionOptions.StreamNameParser));
    }

    [Fact]
    public void Rejects_a_missing_type_mapper()
    {
        var options = ValidOptions();
        options.TypeMapper = null!;

        AssertRejected(options, nameof(CosmosDBCatchupSubscriptionOptions.TypeMapper));
    }
}
