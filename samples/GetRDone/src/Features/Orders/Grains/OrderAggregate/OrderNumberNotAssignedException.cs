namespace GetRDone.Orders.Grains.OrderAggregate;

/// <summary>
///     Thrown by an order's reaction when an issued order number was not assigned for a reason that may pass, so the
///     stream provider delivers the event again until it is.
/// </summary>
[Alias("Orders.OrderNumberNotAssignedException"), GenerateSerializer]
public sealed class OrderNumberNotAssignedException : Exception
{
    public OrderNumberNotAssignedException() { }

    public OrderNumberNotAssignedException(string message) : base(message) { }
}
