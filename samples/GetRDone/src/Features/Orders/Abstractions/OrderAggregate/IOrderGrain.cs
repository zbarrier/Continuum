using Continuum.Domain;
using Continuum.Domain.Orleans;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
}


[GenerateSerializer, Immutable]
public abstract class OrderEvent : DomainEvent;