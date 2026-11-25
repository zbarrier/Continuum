namespace GetRDone.Orders.Grains.Notifications;

/// <summary>
///     Thrown by the New Order Notifier when the notice it emailed was not recorded on the order for a reason that may
///     pass, so the stream provider delivers the event again until it is.
/// </summary>
[Alias("Orders.NewOrderNoticeNotRecordedException"), GenerateSerializer]
public sealed class NewOrderNoticeNotRecordedException : Exception
{
    public NewOrderNoticeNotRecordedException() { }

    public NewOrderNoticeNotRecordedException(string message) : base(message) { }
}
