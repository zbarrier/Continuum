using System.Runtime.CompilerServices;

using Continuum.TypeMapping;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Tests;

public sealed class OrderEventTypeMapTests
{
    [Theory]
    [InlineData(typeof(OrderDraftStarted), "Orders.OrderDraftStarted.V1")]
    [InlineData(typeof(RequestAndPropertySaved), "Orders.RequestAndPropertySaved.V1")]
    [InlineData(typeof(BorrowerAndAccessContactSaved), "Orders.BorrowerAndAccessContactSaved.V1")]
    [InlineData(typeof(SupportingDocumentAttached), "Orders.SupportingDocumentAttached.V1")]
    [InlineData(typeof(SupportingDocumentRemoved), "Orders.SupportingDocumentRemoved.V1")]
    public void OrderEvent_IsRegisteredAsADomainEvent(Type eventType, string typeIdentifier)
    {
        RuntimeHelpers.RunModuleConstructor(eventType.Module.ModuleHandle);

        var registration = Assert.Single(
            TypeMapRegistry.GetRegistrations(TypeMapKinds.DomainEvent),
            item => item.Type == eventType);

        Assert.Equal(typeIdentifier, registration.TypeIdentifier);
    }
}
