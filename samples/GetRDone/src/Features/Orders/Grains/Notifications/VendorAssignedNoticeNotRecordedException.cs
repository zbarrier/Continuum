namespace GetRDone.Orders.Grains.Notifications;

/// <summary>
///     Thrown by the Vendor Assignment Notifier when the notice it emailed was not recorded on the order for a reason that
///     may pass, so the stream provider delivers the event again until it is.
/// </summary>
[Alias("Orders.VendorAssignedNoticeNotRecordedException"), GenerateSerializer]
public sealed class VendorAssignedNoticeNotRecordedException : Exception
{
    public VendorAssignedNoticeNotRecordedException() { }

    public VendorAssignedNoticeNotRecordedException(string message) : base(message) { }
}
