using Continuum.Hosting;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;

namespace Continuum.Tests;

public sealed class TypeMappingBuilderExtensionsTests
{
    private const string Connection = "conn";

    [Fact]
    public void Registers_a_mapper_for_every_combination_of_requested_kinds()
    {
        var provider = new ServiceCollection()
            .AddDefaultTypeMapAttributeMappers(Connection, TypeMapKinds.Command | TypeMapKinds.Snapshot)
            .BuildServiceProvider();

        var command = provider.GetRequiredKeyedService<ITypeMapper>(TypeMapKinds.Command.ToTypeMapperKey(Connection));
        var state = provider.GetRequiredKeyedService<ITypeMapper>(TypeMapKinds.Snapshot.ToTypeMapperKey(Connection));
        var both = provider.GetRequiredKeyedService<ITypeMapper>((TypeMapKinds.Command | TypeMapKinds.Snapshot).ToTypeMapperKey(Connection));

        Assert.True(command.IsTypeRegistered<CreateOrder>());
        Assert.False(command.IsTypeRegistered<OrderState>());
        Assert.True(state.IsTypeRegistered<OrderState>());
        Assert.False(state.IsTypeRegistered<CreateOrder>());
        Assert.True(both.IsTypeRegistered<CreateOrder>());
        Assert.True(both.IsTypeRegistered<OrderState>());
        Assert.False(both.IsTypeRegistered<OrderCreated>());
    }

    [Fact]
    public void Does_not_register_kinds_outside_the_requested_flags()
    {
        var provider = new ServiceCollection()
            .AddDefaultTypeMapAttributeMappers(Connection, TypeMapKinds.Command | TypeMapKinds.Snapshot)
            .BuildServiceProvider();

        Assert.Null(provider.GetKeyedService<ITypeMapper>(TypeMapKinds.DomainEvent.ToTypeMapperKey(Connection)));
        Assert.Null(provider.GetKeyedService<ITypeMapper>((TypeMapKinds.Command | TypeMapKinds.DomainEvent).ToTypeMapperKey(Connection)));
    }

    [Fact]
    public void Connection_name_resolves_to_the_full_combination_mapper()
    {
        var kinds = TypeMapKinds.Command | TypeMapKinds.DomainEvent;
        var provider = new ServiceCollection()
            .AddDefaultTypeMapAttributeMappers(Connection, kinds)
            .BuildServiceProvider();

        var byConnection = provider.GetRequiredKeyedService<ITypeMapper>(Connection);

        Assert.Same(provider.GetRequiredKeyedService<ITypeMapper>(kinds.ToTypeMapperKey(Connection)), byConnection);
    }

    [Fact]
    public void Mapper_is_a_singleton_per_key()
    {
        var provider = new ServiceCollection()
            .AddDefaultTypeMapAttributeMappers(Connection, TypeMapKinds.Command)
            .BuildServiceProvider();
        var key = TypeMapKinds.Command.ToTypeMapperKey(Connection);

        Assert.Same(provider.GetRequiredKeyedService<ITypeMapper>(key), provider.GetRequiredKeyedService<ITypeMapper>(key));
        Assert.True(provider.GetRequiredKeyedService<ITypeMapper>(key).IsFrozen);
    }

    [Fact]
    public void Full_combination_mapper_is_built_at_registration()
    {
        var services = new ServiceCollection()
            .AddDefaultTypeMapAttributeMappers(Connection, TypeMapKinds.Command | TypeMapKinds.DomainEvent);

        var byConnection = Assert.Single(services, d => Equals(d.ServiceKey, Connection));
        var mapper = Assert.IsAssignableFrom<ITypeMapper>(byConnection.KeyedImplementationInstance);
        Assert.True(mapper.IsFrozen);
        Assert.True(mapper.IsTypeRegistered<CreateOrder>());
        Assert.True(mapper.IsTypeRegistered<OrderCreated>());

        var commandOnly = Assert.Single(services, d => Equals(d.ServiceKey, TypeMapKinds.Command.ToTypeMapperKey(Connection)));
        Assert.Null(commandOnly.KeyedImplementationInstance);
        Assert.NotNull(commandOnly.KeyedImplementationFactory);
    }
}
