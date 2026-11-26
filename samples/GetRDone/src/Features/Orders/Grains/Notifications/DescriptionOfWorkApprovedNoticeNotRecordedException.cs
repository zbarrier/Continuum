namespace GetRDone.Orders.Grains.Notifications;

/// <summary>
///     Thrown by the Description of Work Approved Notifier when the notice it emailed was not recorded on the order for a
///     reason that may pass, so the stream provider delivers the event again until it is.
/// </summary>
[Alias("Orders.DescriptionOfWorkApprovedNoticeNotRecordedException"), GenerateSerializer]
public sealed class DescriptionOfWorkApprovedNoticeNotRecordedException : Exception
{
    public DescriptionOfWorkApprovedNoticeNotRecordedException() { }

    public DescriptionOfWorkApprovedNoticeNotRecordedException(string message) : base(message) { }
}
