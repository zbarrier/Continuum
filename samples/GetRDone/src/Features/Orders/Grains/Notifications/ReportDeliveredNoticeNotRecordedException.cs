namespace GetRDone.Orders.Grains.Notifications;

/// <summary>
///     Thrown by the Report Delivered Notifier when the notice it emailed was not recorded on the order for a reason that may
///     pass, so the stream provider delivers the event again until it is.
/// </summary>
[Alias("Orders.ReportDeliveredNoticeNotRecordedException"), GenerateSerializer]
public sealed class ReportDeliveredNoticeNotRecordedException : Exception
{
    public ReportDeliveredNoticeNotRecordedException() { }

    public ReportDeliveredNoticeNotRecordedException(string message) : base(message) { }
}
