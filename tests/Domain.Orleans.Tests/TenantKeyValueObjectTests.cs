using Continuum.CSharpFunctionalExtensions;

namespace Continuum.Domain.Orleans.Tests;

public class TenantKeyValueObjectTests
{
    private sealed record ItemId : TenantKeyValueObject<ItemId>
    {
        public static Result<ItemId> Create(string? value, string? tenantId, string propertyName = nameof(ItemId))
            => Result.NotNullOrWhitespace(value, propertyName)
                .IsTenantKey(tenantId, propertyName)
                .Map(validatedValue => new ItemId(validatedValue));

        private ItemId(string value) : base(value) { }
    }

    private sealed record UserId : TenantKeyValueObject<UserId>
    {
        public static Result<UserId> Create(string? value, string? tenantId, PropertyPath propertyPath, string member)
            => Result.NotNullOrWhitespace(value, propertyPath, member)
                .IsTenantGuidKey(tenantId, propertyPath, member)
                .Map(validatedValue => new UserId(validatedValue));

        private UserId(string value) : base(value) { }
    }

    [Fact]
    public void IsTenantKey_AcceptsAKeyOfTheExpectedTenant()
    {
        var id = ItemId.Create("acme|a1b2", "acme").Value;

        Assert.Equal("acme|a1b2", id.ToString());
        Assert.Equal("acme", id.TenantId);
        Assert.Equal("a1b2", id.Key);
        string text = id;
        Assert.Equal("acme|a1b2", text);
    }

    [Fact]
    public void IsTenantKey_AcceptsAPlainKeyWhenThereIsNoTenant()
    {
        var id = ItemId.Create("a1b2", tenantId: null).Value;

        Assert.Null(id.TenantId);
        Assert.Equal("a1b2", id.Key);
    }

    [Theory]
    [InlineData("acme|~x")]
    [InlineData("acme|~~x")]
    public void IsTenantKey_NormalizesTheKey(string value)
    {
        var id = ItemId.Create(value, "acme").Value;

        Assert.Equal("acme|~~x", id.ToString());
        Assert.Equal("~x", id.Key);
    }

    [Theory]
    [InlineData("other|a1b2", "acme")]
    [InlineData("a1b2", "acme")]
    [InlineData("acme|a1b2", null)]
    public void IsTenantKey_RejectsAKeyOfAnotherTenant(string value, string? tenantId)
    {
        var result = ItemId.Create(value, tenantId);

        var entry = Assert.Single(Assert.IsType<ValidationError>(result.Error).Entries);
        Assert.Equal(nameof(ItemId), entry.Target);
        Assert.Equal(ValidationErrorCodes.InvalidFormat, entry.Code);
    }

    [Theory]
    [InlineData("|a1b2")]
    [InlineData("acme|")]
    [InlineData("acme|a1|b2")]
    public void IsTenantKey_RejectsAMalformedKey(string value)
    {
        var result = ItemId.Create(value, "acme");

        Assert.Equal(ValidationErrorCodes.InvalidFormat, Assert.Single(Assert.IsType<ValidationError>(result.Error).Entries).Code);
    }

    [Fact]
    public void IsTenantKey_PropagatesAnEarlierFailure()
    {
        var result = ItemId.Create(" ", "acme");

        Assert.Equal(ValidationErrorCodes.NotEmpty, Assert.Single(Assert.IsType<ValidationError>(result.Error).Entries).Code);
    }

    [Fact]
    public void IsTenantGuidKey_NormalizesTheGuid()
    {
        var id = UserId.Create("acme|{3F2A9C1E-0B1D-4C8E-9A51-6D2E7F0A1B2C}", "acme", PropertyPath.Root, "User").Value;

        Assert.Equal("acme|3f2a9c1e-0b1d-4c8e-9a51-6d2e7f0a1b2c", id.ToString());
        Assert.Equal(UserId.Create(id, "acme", PropertyPath.Root, "User").Value, id);
    }

    [Theory]
    [InlineData("acme|7304", ValidationErrorCodes.InvalidFormat)]
    [InlineData("acme|00000000-0000-0000-0000-000000000000", ValidationErrorCodes.NotEmpty)]
    [InlineData("other|3f2a9c1e-0b1d-4c8e-9a51-6d2e7f0a1b2c", ValidationErrorCodes.InvalidFormat)]
    public void IsTenantGuidKey_RejectsAKeyThatIsNotANonEmptyGuidOfTheTenant(string value, string code)
    {
        var result = UserId.Create(value, "acme", PropertyPath.Root, "User");

        var entry = Assert.Single(Assert.IsType<ValidationError>(result.Error).Entries);
        Assert.Equal("User", entry.Target);
        Assert.Equal(code, entry.Code);
    }
}
