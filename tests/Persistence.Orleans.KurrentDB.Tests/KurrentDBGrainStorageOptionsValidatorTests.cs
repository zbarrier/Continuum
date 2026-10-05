using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Runtime;

namespace Continuum.Persistence.Orleans.KurrentDB.Tests;

[Collection(ClusterCollection.Name)]
public class KurrentDBGrainStorageOptionsValidatorTests(ClusterFixture fixture)
{
    private IServiceProvider SiloServices => (fixture.Cluster.Primary as global::Orleans.TestingHost.InProcessSiloHandle
        ?? throw new InvalidOperationException("The primary silo is not running in process.")).SiloHost.Services;

    private KurrentDBGrainStorageOptions ValidOptions()
    {
        var resolved = SiloServices.GetRequiredService<IOptionsMonitor<KurrentDBGrainStorageOptions>>().Get(TestSiloConfigurations.MarkerProvider);
        return new KurrentDBGrainStorageOptions
        {
            ConnectionName = resolved.ConnectionName,
            GrainStorageSerializer = resolved.GrainStorageSerializer,
            TypeMapper = resolved.TypeMapper,
        };
    }

    private static void Validate(KurrentDBGrainStorageOptions options) =>
        new KurrentDBGrainStorageOptionsValidator(options, "test").ValidateConfiguration();

    [Fact]
    public void Valid_Options_Pass() => Validate(ValidOptions());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_ConnectionName_Is_Rejected(string? value)
    {
        var options = ValidOptions();
        options.ConnectionName = value!;
        Assert.Throws<OrleansConfigurationException>(() => Validate(options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_Positive_MaxStateEventCount_Is_Rejected(int value)
    {
        var options = ValidOptions();
        options.MaxStateEventCount = value;
        Assert.Throws<OrleansConfigurationException>(() => Validate(options));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    public void Null_Or_Positive_MaxStateEventCount_Passes(int? value)
    {
        var options = ValidOptions();
        options.MaxStateEventCount = value;
        Validate(options);
    }

    [Fact]
    public void Incomplete_Explicit_Credentials_Are_Rejected()
    {
        var options = ValidOptions();
        options.Credentials = new() { UseDefault = false, Username = "admin" };
        Assert.Throws<OrleansConfigurationException>(() => Validate(options));
    }

    [Fact]
    public void Missing_GrainStorageSerializer_Is_Rejected()
    {
        var options = ValidOptions();
        options.GrainStorageSerializer = null!;
        Assert.Throws<OrleansConfigurationException>(() => Validate(options));
    }

    [Fact]
    public void Missing_StreamNameFormatter_Is_Rejected()
    {
        var options = ValidOptions();
        options.StreamNameFormatter = null!;
        Assert.Throws<OrleansConfigurationException>(() => Validate(options));
    }

    [Fact]
    public void Missing_EventIdGenerator_Is_Rejected()
    {
        var options = ValidOptions();
        options.EventIdGenerator = null!;
        Assert.Throws<OrleansConfigurationException>(() => Validate(options));
    }

    [Fact]
    public void Missing_TypeMapper_Is_Rejected()
    {
        var options = ValidOptions();
        options.TypeMapper = null!;
        Assert.Throws<OrleansConfigurationException>(() => Validate(options));
    }
}
