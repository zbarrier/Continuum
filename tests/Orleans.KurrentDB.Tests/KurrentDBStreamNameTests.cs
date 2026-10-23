using Continuum.Orleans.Tenancy;

namespace Continuum.Orleans.KurrentDB.Tests;

public class KurrentDBStreamNameTests
{
    [Theory]
    [InlineData("Order", "3f2a9c1e-0b1d-4c8e", "Order-3f2a9c1e-0b1d-4c8e")]
    [InlineData("Order", "acme|3f2a9c1e-0b1d-4c8e", "Order-acme|3f2a9c1e-0b1d-4c8e")]
    public void Format_JoinsCategoryAndOrleansKey(string category, string orleansKey, string expected)
    {
        Assert.Equal(expected, KurrentDBStreamName.Format(category, orleansKey));
    }

    [Fact]
    public void Format_UsesTenantKeyAsIs()
    {
        var orleansKey = TenantKey.Create("acme-eu", "~key");

        Assert.Equal($"Order-{orleansKey}", KurrentDBStreamName.Format("Order", orleansKey));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Order-Line")]
    [InlineData("Order|x")]
    public void Format_InvalidCategory_Throws(string category)
    {
        Assert.ThrowsAny<ArgumentException>(() => KurrentDBStreamName.Format(category, "key"));
    }

    [Fact]
    public void Format_EmptyKey_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => KurrentDBStreamName.Format("Order", ""));
    }
}
