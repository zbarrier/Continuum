using Continuum.Domain;
using Continuum.Domain.Orleans;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenGrain
{
}


[GenerateSerializer, Immutable]
public abstract class OrderEvent(Guid initiatedBy, DateTimeOffset occurredAt) : DomainEvent(initiatedBy, occurredAt);