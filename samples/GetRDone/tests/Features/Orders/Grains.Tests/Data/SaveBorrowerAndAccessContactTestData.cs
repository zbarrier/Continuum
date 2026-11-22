using System.Security.Claims;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     Borrower &amp; Access contact tab variations: the commands that save them and the events they record, for the Given of
///     any test that needs a saved tab. Each variation is saved by the user who started the matching
///     <see cref="StartOrderDraftTestData"/> draft (<see cref="Command1"/> on <c>DraftStarted1</c>, and so on). Each event is
///     built from its command, so the two never drift apart; the commands use digits-only phone numbers, as recorded.
/// </summary>
public static class SaveBorrowerAndAccessContactTestData
{
    /// <summary>ClientUser1 saves a borrower and an access contact with a phone number.</summary>
    public static SaveBorrowerAndAccessContactCommand Command1 { get; } = new("Daniel Price", "Marcus Doyle", "8285550142");

    /// <summary>ClientUser2 saves only the borrower; the appraiser needs no access contact.</summary>
    public static SaveBorrowerAndAccessContactCommand Command2 { get; } = new("Priya Natarajan", null, null);

    /// <summary>ClientUser1 saves a borrower and a family member as the access contact.</summary>
    public static SaveBorrowerAndAccessContactCommand Command3 { get; } = new("Robert Haines", "Linda Haines", "5405550187");

    /// <summary>The tab <see cref="Command1"/> saves, by ClientUser1.</summary>
    public static BorrowerAndAccessContactSaved Saved1(string orderId, DateTimeOffset? occurredAt = null)
        => Saved(Command1, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>The tab <see cref="Command2"/> saves, by ClientUser2.</summary>
    public static BorrowerAndAccessContactSaved Saved2(string orderId, DateTimeOffset? occurredAt = null)
        => Saved(Command2, orderId, RequestedByUsers.ClientUser2, occurredAt);

    /// <summary>The tab <see cref="Command3"/> saves, by ClientUser1.</summary>
    public static BorrowerAndAccessContactSaved Saved3(string orderId, DateTimeOffset? occurredAt = null)
        => Saved(Command3, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>
    ///     The event the order grain records when <paramref name="initiatedBy"/> saves the tab with <paramref name="command"/>;
    ///     use it directly for a one-off variation, such as <c>Command1 with { AccessContactName = ... }</c>.
    /// </summary>
    public static BorrowerAndAccessContactSaved Saved(
        SaveBorrowerAndAccessContactCommand command, string orderId, ClaimsPrincipal initiatedBy, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        BorrowerName = command.BorrowerName,
        AccessContactName = command.AccessContactName,
        AccessContactPhone = command.AccessContactPhone,
        InitiatedBy = initiatedBy.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };
}
