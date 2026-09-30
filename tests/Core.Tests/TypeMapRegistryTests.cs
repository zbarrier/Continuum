using Continuum.TypeMapping;

namespace Continuum.Tests;

public sealed class TypeMapRegistryTests
{
    private static TypeMapRegistration Find(Type type, TypeMapKinds kinds = (TypeMapKinds)31)
        => TypeMapRegistry.GetRegistrations(kinds).Single(r => r.Type == type);

    [Theory]
    [InlineData(typeof(CreateOrder), "tests.create-order", TypeMapKinds.Command)]
    [InlineData(typeof(OrderCreated), "tests.order-created", TypeMapKinds.DomainEvent)]
    [InlineData(typeof(OrderShipped), "tests.order-shipped", TypeMapKinds.IntegrationEvent)]
    [InlineData(typeof(OrderState), "tests.order-state", TypeMapKinds.Snapshot)]
    [InlineData(typeof(InternalEvent), "tests.internal-event", TypeMapKinds.DomainEvent)]
    [InlineData(typeof(Outer.NestedEvent), "tests.nested-event", TypeMapKinds.DomainEvent)]
    public void Generator_registers_attributed_types(Type type, string identifier, TypeMapKinds kind)
    {
        var registration = Find(type);

        Assert.Equal(identifier, registration.TypeIdentifier);
        Assert.Equal(kind, registration.Kind);
    }

    [Fact]
    public void Unattributed_types_are_not_registered()
    {
        Assert.DoesNotContain(TypeMapRegistry.GetRegistrations((TypeMapKinds)31), r => r.Type == typeof(Unmapped));
    }

    [Fact]
    public void GetRegistrations_filters_by_kind()
    {
        var registrations = TypeMapRegistry.GetRegistrations(TypeMapKinds.Command | TypeMapKinds.Snapshot);

        Assert.Contains(registrations, r => r.Type == typeof(CreateOrder));
        Assert.Contains(registrations, r => r.Type == typeof(OrderState));
        Assert.DoesNotContain(registrations, r => r.Type == typeof(OrderCreated));
        Assert.All(registrations, r => Assert.True(r.Kind is TypeMapKinds.Command or TypeMapKinds.Snapshot));
    }

    [Fact]
    public void Register_same_registration_again_is_noop()
    {
        TypeMapRegistry.Register(typeof(CreateOrder), "tests.create-order", TypeMapKinds.Command);

        Assert.Equal("tests.create-order", Find(typeof(CreateOrder)).TypeIdentifier);
    }

    [Fact]
    public void Register_conflicting_identifier_throws()
    {
        Assert.Throws<ArgumentException>(() => TypeMapRegistry.Register(typeof(CreateOrder), "tests.other", TypeMapKinds.Command));
    }

    [Fact]
    public void Register_conflicting_kind_throws()
    {
        Assert.Throws<ArgumentException>(() => TypeMapRegistry.Register(typeof(CreateOrder), "tests.create-order", TypeMapKinds.Snapshot));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_blank_identifier_throws(string identifier)
    {
        Assert.Throws<ArgumentException>(() => TypeMapRegistry.Register(typeof(Unmapped), identifier, TypeMapKinds.Command));
    }
}
