namespace GetRDone.Orders.Grains.Notifications;

/// <summary>
///     Thrown by the Description of Work Approval Notifier when the notice it emailed was not recorded on the order for a
///     reason that may pass, so the stream provider delivers the event again until it is.
/// </summary>
[Alias("Orders.DescriptionOfWorkSubmittedNoticeNotRecordedException"), GenerateSerializer]
public sealed class DescriptionOfWorkSubmittedNoticeNotRecordedException : Exception
{
    public DescriptionOfWorkSubmittedNoticeNotRecordedException() { }

    public DescriptionOfWorkSubmittedNoticeNotRecordedException(string message) : base(message) { }
}
