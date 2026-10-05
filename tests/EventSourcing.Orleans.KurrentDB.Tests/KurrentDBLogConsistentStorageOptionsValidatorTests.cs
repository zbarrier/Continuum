using Continuum.TypeMapping;

using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests;

public class KurrentDBLogConsistentStorageOptionsValidatorTests
{
    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private static KurrentDBLogConsistentStorageOptions ValidOptions() => new()
    {
        ConnectionName = "kurrent",
        GrainStorageSerializer = new FakeSerializer(),
        TypeMapper = new FakeTypeMapper(),
    };

    private static void Validate(KurrentDBLogConsistentStorageOptions options) =>
        new KurrentDBLogConsistentStorageOptionsValidator(options, "provider").ValidateConfiguration();

    private static void AssertInvalid(KurrentDBLogConsistentStorageOptions options, string expected)
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
        var exception = Assert.Throws<OrleansConfigurationException>(() => new KurrentDBLogConsistentStorageOptionsValidator(null!, "provider"));
        Assert.Contains("provider", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_ConnectionName_Is_Rejected(string? connectionName)
    {
        var options = ValidOptions();
        options.ConnectionName = connectionName!;
        AssertInvalid(options, nameof(options.ConnectionName));
    }

    [Fact]
    public void Missing_Credentials_Are_Rejected()
    {
        var options = ValidOptions();
        options.Credentials = null!;
        AssertInvalid(options, nameof(options.Credentials));
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(" ", null, null)]
    [InlineData(null, "user", null)]
    [InlineData(null, null, "secret")]
    [InlineData(null, "user", " ")]
    public void Explicit_Credentials_Without_A_Token_Or_Login_Are_Rejected(string? authToken, string? username, string? password)
    {
        var options = ValidOptions();
        options.Credentials = new() { UseDefault = false, AuthToken = authToken, Username = username, Password = password };
        AssertInvalid(options, "requires an AuthToken or Username and Password");
    }

    [Theory]
    [InlineData("token", null, null)]
    [InlineData(null, "user", "secret")]
    public void Explicit_Credentials_With_A_Token_Or_Login_Pass(string? authToken, string? username, string? password)
    {
        var options = ValidOptions();
        options.Credentials = new() { UseDefault = false, AuthToken = authToken, Username = username, Password = password };
        Validate(options);
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
