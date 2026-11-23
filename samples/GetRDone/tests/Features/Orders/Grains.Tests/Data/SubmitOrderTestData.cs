using System.Security.Claims;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     Order submissions: the events that submit the <see cref="StartOrderDraftTestData"/> drafts, each by the user who
///     started the draft (<see cref="Submitted1"/> submits <c>DraftStarted1</c>, and so on), for the client it was started for.
/// </summary>
public static class SubmitOrderTestData
{
    /// <summary>The submission of <c>DraftStarted1</c>, by ClientUser1.</summary>
    public static OrderSubmitted Submitted1(string orderId, DateTimeOffset? occurredAt = null)
        => Submitted(StartOrderDraftTestData.Command1, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>The submission of <c>DraftStarted2</c>, by ClientUser2.</summary>
    public static OrderSubmitted Submitted2(string orderId, DateTimeOffset? occurredAt = null)
        => Submitted(StartOrderDraftTestData.Command2, orderId, RequestedByUsers.ClientUser2, occurredAt);

    /// <summary>The submission of <c>DraftStarted3</c>, by ClientUser1.</summary>
    public static OrderSubmitted Submitted3(string orderId, DateTimeOffset? occurredAt = null)
        => Submitted(StartOrderDraftTestData.Command3, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>The event the order grain records when <paramref name="initiatedBy"/> submits the draft <paramref name="draft"/> started.</summary>
    public static OrderSubmitted Submitted(
        StartOrderDraftCommand draft, string orderId, ClaimsPrincipal initiatedBy, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        ClientId = draft.ClientId,
        InitiatedBy = initiatedBy.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };
}
