using Continuum.TypeMapping;

using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests;

public class CosmosDBLogConsistentStorageOptionsValidatorTests
{
    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private static CosmosDBLogConsistentStorageOptions ValidOptions() => new()
    {
        ConnectionName = "cosmos",
        DatabaseName = "orleans",
        ContainerName = "events",
        GrainStorageSerializer = new FakeSerializer(),
        TypeMapper = new FakeTypeMapper(),
    };

    private static void Validate(CosmosDBLogConsistentStorageOptions options) =>
        new CosmosDBLogConsistentStorageOptionsValidator(options, "provider").ValidateConfiguration();

    private static void AssertInvalid(CosmosDBLogConsistentStorageOptions options, string expected)
    {
        var exception = Assert.Throws<OrleansConfigurationException>(() => Validate(options));
        Assert.Contains("provider", exception.Message);
        Assert.Contains(expected, exception.Message);
    }

    [Fact]
    public void Valid_Options_Pass()
    {
        Validate(ValidOptions());
    }

    [Fact]
    public void Null_Options_Are_Rejected()
    {
        var exception = Assert.Throws<OrleansConfigurationException>(() => new CosmosDBLogConsistentStorageOptionsValidator(null!, "provider"));
        Assert.Contains("provider", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_ConnectionName_Is_Rejected(string? value)
    {
        var options = ValidOptions();
        options.ConnectionName = value!;
        AssertInvalid(options, nameof(options.ConnectionName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_DatabaseName_Is_Rejected(string? value)
    {
        var options = ValidOptions();
        options.DatabaseName = value!;
        AssertInvalid(options, nameof(options.DatabaseName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_ContainerName_Is_Rejected(string? value)
    {
        var options = ValidOptions();
        options.ContainerName = value!;
        AssertInvalid(options, nameof(options.ContainerName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(101)]
    public void Out_Of_Range_BatchSize_Is_Rejected(int batchSize)
    {
        var options = ValidOptions();
        options.BatchSize = batchSize;
        AssertInvalid(options, nameof(options.BatchSize));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(100)]
    public void In_Range_BatchSize_Passes(int batchSize)
    {
        var options = ValidOptions();
        options.BatchSize = batchSize;
        Validate(options);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_Positive_QueryMaxItemCount_Is_Rejected(int queryMaxItemCount)
    {
        var options = ValidOptions();
        options.QueryMaxItemCount = queryMaxItemCount;
        AssertInvalid(options, nameof(options.QueryMaxItemCount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_Positive_StartupConnectionTimeout_Is_Rejected(int seconds)
    {
        var options = ValidOptions();
        options.StartupConnectionTimeout = TimeSpan.FromSeconds(seconds);
        AssertInvalid(options, nameof(options.StartupConnectionTimeout));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Invalid_RequestChargeWarningThreshold_Is_Rejected(double threshold)
    {
        var options = ValidOptions();
        options.RequestChargeWarningThreshold = threshold;
        AssertInvalid(options, nameof(options.RequestChargeWarningThreshold));
    }

    [Fact]
    public void RequestChargeWarningThreshold_Defaults_To_50()
    {
        Assert.Equal(50, new CosmosDBLogConsistentStorageOptions().RequestChargeWarningThreshold);
    }

    [Fact]
    public void Missing_GrainStorageSerializer_Is_Rejected()
    {
        var options = ValidOptions();
        options.GrainStorageSerializer = null!;
        AssertInvalid(options, nameof(options.GrainStorageSerializer));
    }

    [Fact]
    public void Missing_StreamNameFormatter_Is_Rejected()
    {
        var options = ValidOptions();
        options.StreamNameFormatter = null!;
        AssertInvalid(options, nameof(options.StreamNameFormatter));
    }

    [Fact]
    public void Missing_EventIdGenerator_Is_Rejected()
    {
        var options = ValidOptions();
        options.EventIdGenerator = null!;
        AssertInvalid(options, nameof(options.EventIdGenerator));
    }

    [Fact]
    public void Missing_TypeMapper_Is_Rejected()
    {
        var options = ValidOptions();
        options.TypeMapper = null!;
        AssertInvalid(options, nameof(options.TypeMapper));
    }
}
