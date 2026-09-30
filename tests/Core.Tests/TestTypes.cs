using Continuum.TypeMapping;

namespace Continuum.Tests;

[CommandType("tests.create-order")]
public sealed class CreateOrder;

[DomainEventType("tests.order-created")]
public sealed class OrderCreated;

[IntegrationEventType("tests.order-shipped")]
public sealed class OrderShipped;

[SnapshotType("tests.order-state")]
public sealed class OrderState;

[DomainEventType("tests.internal-event")]
internal sealed class InternalEvent;

public static class Outer
{
    [DomainEventType("tests.nested-event")]
    public sealed class NestedEvent;
}

public sealed class Unmapped;

public sealed class TestTypeMapper : TypeMapper;
