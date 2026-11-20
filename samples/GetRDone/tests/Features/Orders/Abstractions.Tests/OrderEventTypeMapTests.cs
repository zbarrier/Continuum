using System.Runtime.CompilerServices;

using Continuum.TypeMapping;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Tests;

public sealed class OrderEventTypeMapTests
{
    [Fact]
    public void OrderDraftStarted_IsRegisteredAsADomainEvent()
    {
        RuntimeHelpers.RunModuleConstructor(typeof(OrderDraftStarted).Module.ModuleHandle);

        var registration = Assert.Single(
            TypeMapRegistry.GetRegistrations(TypeMapKinds.DomainEvent),
            item => item.Type == typeof(OrderDraftStarted));

        Assert.Equal("GetRDone.Orders.OrderDraftStarted", registration.TypeIdentifier);
    }
}
