using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>An entry on an order's timeline as the projections store it.</summary>
/// <param name="OccurredAt">When it happened.</param>
/// <param name="Kind">What happened.</param>
/// <param name="UserId">
///     Who submitted, was assigned, acknowledged the order or provided a document, as a tenant user ID.
/// </param>
/// <param name="OrderNumber">The number the order was given.</param>
/// <param name="DocumentTypeName">The type of document requested or provided.</param>
[GenerateSerializer, Immutable]
public sealed record OrderTimelineItem(
    DateTimeOffset OccurredAt,
    OrderTimelineKind Kind,
    string? UserId,
    long? OrderNumber,
    string? DocumentTypeName = null);

public enum OrderTimelineKind
{
    Submitted,
    OrderNumberAssigned,
    Assigned,
    Acknowledged,
    DocumentRequested,
    DocumentProvided
}

/// <summary>Turns the timeline the projections store into what the staff and client order pages show.</summary>
internal static class OrderTimeline
{
    /// <summary>The order's timeline, newest first; entries at the same time keep the order they happened in, reversed.</summary>
    public static OrderTimelineEntry[] Describe(IReadOnlyList<OrderTimelineItem> items, UserDirectoryNames names)
        => items
            .Select((item, index) => (Item: item, Index: index))
            .OrderByDescending(entry => entry.Item.OccurredAt)
            .ThenByDescending(entry => entry.Index)
            .Select(entry => new OrderTimelineEntry(entry.Item.OccurredAt, Describe(entry.Item, names)))
            .ToArray();

    private static string Describe(OrderTimelineItem item, UserDirectoryNames names)
    {
        var userName = names.UserName(item.UserId) ?? "an unknown user";
        return item.Kind switch
        {
            OrderTimelineKind.Submitted => $"Submitted by {userName}",
            OrderTimelineKind.OrderNumberAssigned => $"Order number {item.OrderNumber}",
            OrderTimelineKind.Assigned => $"Assigned to {userName}",
            OrderTimelineKind.Acknowledged => $"Acknowledged by {userName}",
            OrderTimelineKind.DocumentRequested => $"Document requested: {item.DocumentTypeName}",
            OrderTimelineKind.DocumentProvided => $"{item.DocumentTypeName} provided by {userName}",
            _ => item.Kind.ToString()
        };
    }
}
