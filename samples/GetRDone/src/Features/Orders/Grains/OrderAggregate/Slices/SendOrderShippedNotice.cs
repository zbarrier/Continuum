using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<SendOrderShippedNoticeResponse>> SendOrderShippedNoticeAsync(SendOrderShippedNoticeCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(OrderShippedNoticeSent.InitiatedBy))
                        .Deconstruct(out var initiatedBy),
                    DocumentId.Create(command.DocumentId, tenantId, nameof(command.DocumentId)).Deconstruct(out var documentId),
                    UserId.Create(command.RecipientUserId, tenantId, nameof(command.RecipientUserId)).Deconstruct(out var recipientUserId),
                    EmailAddress.Create(command.RecipientEmail, nameof(command.RecipientEmail)).Deconstruct(out var recipientEmail),
                    EmailSubject.Create(command.Subject, nameof(command.Subject)).Deconstruct(out var subject))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendOrderShippedNotice(initiatedBy!))
                .Ensure(() => State.Status == OrderStatus.Shipped && State.Report?.DocumentId == (string)documentId!,
                    () => RequestErrors.NewNotFound("The shipped report was not found."))
                .Ensure(() => State.StartedBy == (string)recipientUserId!,
                    () => RequestErrors.NewInvalidArg("The notice must go to the client who entered the order."))
                .BindWithStreamTransaction(this, transaction =>
                {
                    // The notice is sent once; another attempt is an earlier attempt of the automation.
                    if (State.OrderShippedNoticeSent)
                    {
                        return;
                    }
                    transaction.Apply(new OrderShippedNoticeSent
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
                .Map(response => new SendOrderShippedNoticeResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }

    public async Task<Result<bool>> IsOrderShippedNoticeSentAsync(string documentId)
    {
        return await Result.Success().BindTry(() =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return Task.FromResult(Result.CombineForValidation(
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId).Deconstruct(out var userId),
                    DocumentId.Create(documentId, tenantId, nameof(documentId)).Deconstruct(out var validId))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendOrderShippedNotice(userId!))
                .Ensure(() => State.Status == OrderStatus.Shipped && State.Report?.DocumentId == (string)validId!,
                    () => RequestErrors.NewNotFound("The shipped report was not found."))
                .Map(() => State.OrderShippedNoticeSent));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(OrderShippedNoticeSent evt) => OrderShippedNoticeSent = true;
}
