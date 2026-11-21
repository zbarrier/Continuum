namespace GetRDone.Web.Components.Client;

/// <summary>A submitted order on the client's My orders list, as described by the ClientOrders read model.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="OrderNumber">The order number assigned when the order was submitted.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="FormTypeName">The form type ordered.</param>
/// <param name="Status">The order's status as the client sees it.</param>
/// <param name="SubmittedAt">When the order was submitted.</param>
public sealed record ClientOrderRow(
    string OrderId,
    long OrderNumber,
    string PropertyAddress,
    string FormTypeName,
    string Status,
    DateTimeOffset SubmittedAt);
