using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<SendReportDeliveredNoticeResponse>> SendReportDeliveredNoticeAsync(SendReportDeliveredNoticeCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(ReportDeliveredNoticeSent.InitiatedBy))
                        .Deconstruct(out var initiatedBy),
                    DocumentId.Create(command.DocumentId, tenantId, nameof(command.DocumentId)).Deconstruct(out var documentId),
                    UserId.Create(command.RecipientUserId, tenantId, nameof(command.RecipientUserId)).Deconstruct(out var recipientUserId),
                    EmailAddress.Create(command.RecipientEmail, nameof(command.RecipientEmail)).Deconstruct(out var recipientEmail),
                    EmailSubject.Create(command.Subject, nameof(command.Subject)).Deconstruct(out var subject))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendReportDeliveredNotice(initiatedBy!))
                .Ensure(() => State.Report?.DocumentId == (string)documentId!,
                    () => RequestErrors.NewNotFound("The report was not found."))
                .Ensure(() => State.AssignedTo == (string)recipientUserId!,
                    () => RequestErrors.NewInvalidArg("The notice must go to the staff member the order is assigned to."))
                .BindWithStreamTransaction(this, transaction =>
                {
                    // The notice is sent once; another attempt is an earlier attempt of the automation.
                    if (State.Report!.NoticeSent)
                    {
                        return;
                    }
                    transaction.Apply(new ReportDeliveredNoticeSent
                    {
                        OrderId = orderId!,
                        DocumentId = documentId!,
                        RecipientUserId = recipientUserId!,
                        RecipientEmail = recipientEmail!,
                        Subject = subject!,
                        InitiatedBy = initiatedBy!,
                        OccurredAt = DateTimeOffset.UtcNow
                    });
                })
                .Map(response => new SendReportDeliveredNoticeResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }

    public async Task<Result<bool>> IsReportDeliveredNoticeSentAsync(string documentId)
    {
        return await Result.Success().BindTry(() =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return Task.FromResult(Result.CombineForValidation(
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId).Deconstruct(out var userId),
                    DocumentId.Create(documentId, tenantId, nameof(documentId)).Deconstruct(out var validId))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendReportDeliveredNotice(userId!))
                .Ensure(() => State.Report?.DocumentId == (string)validId!,
                    () => RequestErrors.NewNotFound("The report was not found."))
                .Map(() => State.Report!.NoticeSent));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(ReportDeliveredNoticeSent evt)
        => Report = Report! with { NoticeSent = true };
}
