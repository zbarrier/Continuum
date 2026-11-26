namespace GetRDone.Orders.Grains.Notifications;

/// <summary>
///     Thrown by the Order Shipped Notifier when the notice it emailed was not recorded on the order for a reason that may
///     pass, so the stream provider delivers the event again until it is.
/// </summary>
[Alias("Orders.OrderShippedNoticeNotRecordedException"), GenerateSerializer]
public sealed class OrderShippedNoticeNotRecordedException : Exception
{
    public OrderShippedNoticeNotRecordedException() { }

    public OrderShippedNoticeNotRecordedException(string message) : base(message) { }
}
