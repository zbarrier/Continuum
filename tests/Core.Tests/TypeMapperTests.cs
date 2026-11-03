using Continuum.TypeMapping;

namespace Continuum.Tests;

public sealed class TypeMapperTests
{
    [Fact]
    public void RegisterKnownAttributeTypes_adds_only_requested_kinds()
    {
        var mapper = new DefaultTypeMapAttributeMapper();

        mapper.RegisterKnownTypes(TypeMapKinds.DomainEvent);

        Assert.Equal("tests.order-created", mapper.GetTypeName<OrderCreated>());
        Assert.Equal(typeof(OrderCreated), mapper.GetType("tests.order-created"));
        Assert.False(mapper.IsTypeRegistered<CreateOrder>());
    }

    [Fact]
    public void GetTypeName_unknown_type_throws()
    {
        var mapper = new TestTypeMapper();

        Assert.Throws<TypeMappingNotFoundException>(() => mapper.GetTypeName<Unmapped>());
    }

    [Fact]
    public void GetType_unknown_name_throws()
    {
        var mapper = new TestTypeMapper();

        Assert.Throws<TypeMappingNotFoundException>(() => mapper.GetType("missing"));
    }

    [Fact]
    public void TryGet_methods_report_missing_and_present_entries()
    {
        var mapper = new TestTypeMapper();
        mapper.AddType(typeof(Unmapped), "unmapped");

        Assert.True(mapper.TryGetTypeName(typeof(Unmapped), out var name));
        Assert.Equal("unmapped", name);
        Assert.True(mapper.TryGetType("unmapped", out var type));
        Assert.Equal(typeof(Unmapped), type);
        Assert.False(mapper.TryGetTypeName(typeof(CreateOrder), out _));
        Assert.False(mapper.TryGetType("missing", out _));
    }

    [Fact]
    public void AddType_same_mapping_twice_is_noop()
    {
        var mapper = new TestTypeMapper();

        mapper.AddType(typeof(Unmapped), "unmapped");
        mapper.AddType(typeof(Unmapped), "unmapped");

        Assert.Single(mapper.TypeMap);
    }

    [Fact]
    public void AddType_different_name_for_same_type_throws()
    {
        var mapper = new TestTypeMapper();
        mapper.AddType(typeof(Unmapped), "unmapped");

        Assert.Throws<ArgumentException>(() => mapper.AddType(typeof(Unmapped), "other"));
    }

    [Fact]
    public void AddType_same_name_for_different_type_throws_and_leaves_mapper_unchanged()
    {
        var mapper = new TestTypeMapper();
        mapper.AddType(typeof(Unmapped), "shared");

        var ex = Assert.Throws<ArgumentException>(() => mapper.AddType(typeof(CreateOrder), "shared"));

        Assert.Contains(typeof(Unmapped).FullName!, ex.Message);
        Assert.Single(mapper.TypeMap);
        Assert.Single(mapper.TypeNameMap);
        Assert.False(mapper.IsTypeRegistered<CreateOrder>());
    }

    [Fact]
    public void Freeze_keeps_lookups_and_rejects_changes()
    {
        var mapper = new TestTypeMapper();
        mapper.AddType(typeof(Unmapped), "unmapped");

        mapper.Freeze();
        mapper.Freeze();

        Assert.True(mapper.IsFrozen);
        Assert.Equal("unmapped", mapper.GetTypeName<Unmapped>());
        Assert.Equal(typeof(Unmapped), mapper.GetType("unmapped"));
        Assert.True(mapper.IsTypeRegistered<Unmapped>());
        Assert.False(mapper.TryGetType("missing", out _));
        Assert.Single(mapper.TypeMap);
        Assert.Single(mapper.TypeNameMap);
        Assert.Throws<InvalidOperationException>(() => mapper.AddType(typeof(CreateOrder), "create"));
        Assert.Throws<InvalidOperationException>(() => mapper.RemoveType<Unmapped>());
    }

    [Fact]
    public void RemoveType_removes_both_directions()
    {
        var mapper = new TestTypeMapper();
        mapper.AddType(typeof(Unmapped), "unmapped");

        mapper.RemoveType<Unmapped>();

        Assert.Empty(mapper.TypeMap);
        Assert.Empty(mapper.TypeNameMap);
    }

    [Fact]
    public void AddType_generic_maps_both_directions()
    {
        var mapper = new TestTypeMapper();

        mapper.AddType<Unmapped>("unmapped");

        Assert.Equal("unmapped", mapper.GetTypeName<Unmapped>());
        Assert.Equal(typeof(Unmapped), mapper.GetType("unmapped"));
    }

    [Fact]
    public void RemoveType_by_type_removes_both_directions()
    {
        var mapper = new TestTypeMapper();
        mapper.AddType(typeof(Unmapped), "unmapped");

        mapper.RemoveType(typeof(Unmapped));

        Assert.Empty(mapper.TypeMap);
        Assert.Empty(mapper.TypeNameMap);
    }

    [Fact]
    public void RegisterKnownAttributeTypes_always_adds_framework_types()
    {
        var mapper = new DefaultTypeMapAttributeMapper();

        mapper.RegisterKnownTypes(TypeMapKinds.Command);

        Assert.Equal("tests.framework-marker", mapper.GetTypeName<FrameworkMarker>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void AddType_blank_name_throws(string? typeName)
    {
        var mapper = new TestTypeMapper();

        Assert.ThrowsAny<ArgumentException>(() => mapper.AddType(typeof(Unmapped), typeName!));
        Assert.Empty(mapper.TypeMap);
    }

    [Fact]
    public void RemoveType_unmapped_type_throws()
    {
        var mapper = new TestTypeMapper();

        Assert.Throws<TypeMappingNotFoundException>(() => mapper.RemoveType<Unmapped>());
    }

    [Fact]
    public void TypeMappingNotFoundException_names_the_missing_type()
    {
        var mapper = new TestTypeMapper();

        var byType = Assert.Throws<TypeMappingNotFoundException>(() => mapper.GetTypeName<Unmapped>());
        var byName = Assert.Throws<TypeMappingNotFoundException>(() => mapper.GetType("missing"));

        Assert.Equal($"A type mapping for the type '{typeof(Unmapped).FullName}' was not found.", byType.Message);
        Assert.Equal("A type mapping for the type 'missing' was not found.", byName.Message);
    }

    [Fact]
    public void EnsureTypesRegistered_throws_for_missing_type()
    {
        var mapper = new TestTypeMapper();
        mapper.AddType(typeof(Unmapped), "unmapped");

        mapper.EnsureTypesRegistered([typeof(Unmapped)]);
        Assert.Throws<TypeMappingNotFoundException>(() => mapper.EnsureTypesRegistered([typeof(Unmapped), typeof(CreateOrder)]));
    }
}
