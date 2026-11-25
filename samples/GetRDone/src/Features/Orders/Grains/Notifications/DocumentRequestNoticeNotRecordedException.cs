namespace GetRDone.Orders.Grains.Notifications;

/// <summary>
///     Thrown by the Document Request Notifier when the notice it emailed was not recorded on the order for a reason that
///     may pass, so the stream provider delivers the event again until it is.
/// </summary>
[Alias("Orders.DocumentRequestNoticeNotRecordedException"), GenerateSerializer]
public sealed class DocumentRequestNoticeNotRecordedException : Exception
{
    public DocumentRequestNoticeNotRecordedException() { }

    public DocumentRequestNoticeNotRecordedException(string message) : base(message) { }
}
