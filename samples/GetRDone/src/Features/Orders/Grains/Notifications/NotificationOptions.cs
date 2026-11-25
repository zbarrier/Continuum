namespace GetRDone.Orders.Grains.Notifications;

/// <summary>What the notifications need to know about the apps they point people to.</summary>
public sealed class NotificationOptions
{
    /// <summary>
    ///     The base URL of GetRDone.Web, such as <c>https://localhost:7001</c>, which emails link to. With none, the emails
    ///     have no links.
    /// </summary>
    public string? WebBaseUrl { get; set; }
}
