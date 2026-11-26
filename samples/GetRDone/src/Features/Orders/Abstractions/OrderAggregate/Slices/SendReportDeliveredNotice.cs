using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Records that the Report Delivered Notifier told the staff member the order is assigned to that the vendor delivered
    ///     its report. Only the Report Delivered Notifier automation may record it. Succeeds without changes when the notice
    ///     was already sent, so the automation can safely retry.
    /// </summary>
    Task<Result<SendReportDeliveredNoticeResponse>> SendReportDeliveredNoticeAsync(SendReportDeliveredNoticeCommand command);

    /// <summary>
    ///     Whether the notice of the delivered report was sent, so the Report Delivered Notifier doesn't email it again when
    ///     the event that started it is delivered again. Only the Report Delivered Notifier automation may ask.
    /// </summary>
    [ReadOnly]
    Task<Result<bool>> IsReportDeliveredNoticeSentAsync(string documentId);
}

/// <summary>The notice of the delivered report the Report Delivered Notifier emailed: who it went to and its subject.</summary>
[Alias("Orders.SendReportDeliveredNoticeCommand.V1"), GenerateSerializer, Immutable]
public sealed record SendReportDeliveredNoticeCommand(
    string DocumentId,
    string RecipientUserId,
    string RecipientEmail,
    string Subject);

[Alias("Orders.SendReportDeliveredNoticeResponse.V1"), GenerateSerializer, Immutable]
public sealed class SendReportDeliveredNoticeResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

/// <summary>The email telling the staff member the order is assigned to that the vendor delivered its report went out.</summary>
[Alias("Orders.ReportDeliveredNoticeSent.V1"), DomainEventType("Orders.ReportDeliveredNoticeSent.V1"), GenerateSerializer, Immutable]
public sealed class ReportDeliveredNoticeSent : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string DocumentId { get; init; }
    [Id(2)] public required string RecipientUserId { get; init; }
    [Id(3)] public required string RecipientEmail { get; init; }
    [Id(4)] public required string Subject { get; init; }
}
