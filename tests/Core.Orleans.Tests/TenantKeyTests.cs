using System.Reflection;

using Continuum.Orleans.Streams;
using Continuum.Orleans.Tenancy;

using Orleans.Multitenant;
using Orleans.Runtime;

namespace Continuum.Orleans.Tests;

public class TenantKeyTests
{
    public static TheoryData<string?, string> Keys => new()
    {
        { null, "3f2a9c1e-0b1d-4c8e-9a51-6d2e7f0a1b2c" },
        { null, "~starts-with-tilde" },
        { "acme", "3f2a9c1e-0b1d-4c8e-9a51-6d2e7f0a1b2c" },
        { "acme-eu", "order-1" },
        { "acme", "~starts-with-tilde" },
        { "acme", "~~two-tildes" },
        { "acme", "ends-with-tilde~" },
    };

    [Theory]
    [MemberData(nameof(Keys))]
    public void Create_MatchesOrleansMultitenant(string? tenantId, string keyWithinTenant)
    {
        var expected = KeyCapturingGrainFactory.CaptureKey(tenantId, keyWithinTenant);

        Assert.Equal(expected, TenantKey.Create(tenantId, keyWithinTenant));
    }

    [Theory]
    [MemberData(nameof(Keys))]
    public void Split_MatchesOrleansMultitenant(string? tenantId, string keyWithinTenant)
    {
        var key = TenantKey.Create(tenantId, keyWithinTenant);
        var streamId = StreamId.Create("ns", key);

        TenantKey.Split(key, out var actualTenantId, out var actualKey);

        Assert.Equal(streamId.GetTenantId(), actualTenantId);
        Assert.Equal(streamId.GetKeyWithinTenant(), actualKey);
        Assert.Equal(tenantId, actualTenantId);
        Assert.Equal(keyWithinTenant, actualKey);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ac|me")]
    public void Create_InvalidTenantId_Throws(string tenantId)
    {
        Assert.ThrowsAny<ArgumentException>(() => TenantKey.Create(tenantId, "key"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("a|b")]
    [InlineData("|key")]
    public void Create_InvalidKey_Throws(string key)
    {
        Assert.ThrowsAny<ArgumentException>(() => TenantKey.Create("acme", key));
        Assert.ThrowsAny<ArgumentException>(() => TenantKey.Create(null, key));
    }

    [Theory]
    [InlineData("")]
    [InlineData("|key")]
    [InlineData("acme|")]
    public void Split_Invalid_Throws(string key)
    {
        Assert.ThrowsAny<FormatException>(() => TenantKey.Split(key, out _, out _));
    }

    [Theory]
    [InlineData("acme|~", "~")]
    [InlineData("acme|~x", "~x")]
    [InlineData("acme|x~~", "x~~")]
    public void Split_TildeThatIsNotAnEscape_IsKept(string key, string expectedKey)
    {
        TenantKey.Split(key, out var tenantId, out var keyWithinTenant);

        Assert.Equal("acme", tenantId);
        Assert.Equal(expectedKey, keyWithinTenant);
    }

    [Theory]
    [InlineData("")]
    [InlineData("|key")]
    [InlineData("acme|")]
    public void StreamName_Create_EmptyTenantOrKey_Throws(string remainder)
    {
        Assert.Throws<FormatException>(() => StreamNameParsing.Create($"Order-{remainder}", "Order", remainder));
    }

    [Theory]
    [InlineData("acme|3f2a9c1e-0b1d", "acme", "3f2a9c1e-0b1d")]
    [InlineData("3f2a9c1e-0b1d", null, "3f2a9c1e-0b1d")]
    [InlineData("acme|~~key", "acme", "~key")]
    [InlineData("acme|~|key", "acme", "|key")]
    public void StreamName_Create_SplitsTenantAndKey(string remainder, string? tenantId, string key)
    {
        var name = StreamNameParsing.Create($"Order-{remainder}", "Order", remainder);

        Assert.Equal("Order", name.Category);
        Assert.Equal(tenantId, name.TenantId);
        Assert.Equal(key, name.Key);
    }

    public class KeyCapturingGrainFactory : DispatchProxy
    {
        private string? _key;

        public static string CaptureKey(string? tenantId, string keyWithinTenant)
        {
            var factory = Create<IGrainFactory, KeyCapturingGrainFactory>();
            _ = factory.ForTenant(tenantId!).GetGrain<IGrainWithStringKey>(keyWithinTenant);
            return ((KeyCapturingGrainFactory)(object)factory)._key!;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            _key = (string)args![0]!;
            return null;
        }
    }
}
