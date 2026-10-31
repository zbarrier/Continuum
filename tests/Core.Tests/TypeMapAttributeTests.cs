using System.ComponentModel;

using Continuum.TypeMapping;

namespace Continuum.Tests;

public sealed class TypeMapAttributeTests
{
    private sealed class TestAttribute(string typeIdentifier, TypeMapKinds kinds) : TypeMapAttribute(typeIdentifier, kinds);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Blank_identifier_throws(string? identifier)
    {
        var ex = Assert.Throws<ArgumentException>(() => new TestAttribute(identifier!, TypeMapKinds.Command));

        Assert.Equal("typeIdentifier", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(64)]
    [InlineData(-1)]
    public void Kind_that_is_not_exactly_one_defined_kind_throws(int kinds)
    {
        var ex = Assert.Throws<ArgumentException>(() => new TestAttribute("id", (TypeMapKinds)kinds));

        Assert.Equal("typeKinds", ex.ParamName);
    }

    [Fact]
    public void Single_kind_is_kept()
    {
        var attribute = new TestAttribute("id", TypeMapKinds.Framework);

        Assert.Equal("id", attribute.TypeIdentifier);
        Assert.Equal(TypeMapKinds.Framework, attribute.TypeKind);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(63)]
    public void ToTypeMapKinds_accepts_defined_combinations(int value)
    {
        Assert.Equal((TypeMapKinds)value, value.ToTypeMapKinds());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(64)]
    [InlineData(-1)]
    public void ToTypeMapKinds_rejects_out_of_range_values(int value)
    {
        Assert.Throws<InvalidEnumArgumentException>(() => value.ToTypeMapKinds());
    }

    [Fact]
    public void ToTypeMapperKey_combines_connection_and_value()
    {
        Assert.Equal("conn-3", (TypeMapKinds.Command | TypeMapKinds.DomainEvent).ToTypeMapperKey("conn"));
    }
}
