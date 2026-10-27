namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Tests;

/// <summary>
///     Verifies how a CosmosDB stream name decomposes into its category, tenant and key.
/// </summary>
public class DefaultCosmosDBStreamNameParserTests
{
    private readonly DefaultCosmosDBStreamNameParser _parser = new();

    [Fact]
    public void TryParse_Splits_A_Tenant_Qualified_Name()
    {
        Assert.True(_parser.TryParse("orders/acme|123", out var parsed));

        Assert.Equal("orders", parsed.Category);
        Assert.Equal("acme", parsed.TenantId);
        Assert.Equal("123", parsed.Key);
    }

    [Fact]
    public void TryParse_Treats_A_Name_With_No_Separator_As_All_Key()
    {
        Assert.True(_parser.TryParse("123", out var parsed));

        Assert.Equal(string.Empty, parsed.Category);
        Assert.Equal("123", parsed.Key);
    }

    [Theory]
    [InlineData("orders/|123")]
    [InlineData("orders/acme|")]
    [InlineData("orders/")]
    public void TryParse_Returns_False_For_A_Name_Parse_Rejects(string streamName)
    {
        Assert.Throws<FormatException>(() => _parser.Parse(streamName));
        Assert.False(_parser.TryParse(streamName, out var parsed));
        Assert.Null(parsed);
    }
}
