using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<SendDocumentRequestNoticeResponse>> SendDocumentRequestNoticeAsync(SendDocumentRequestNoticeCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(DocumentRequestNoticeSent.InitiatedBy)).Deconstruct(out var initiatedBy),
                    DocumentRequestId.Create(command.RequestId, tenantId, nameof(command.RequestId)).Deconstruct(out var requestId),
                    UserId.Create(command.RecipientUserId, tenantId, nameof(command.RecipientUserId)).Deconstruct(out var recipientUserId),
                    EmailAddress.Create(command.RecipientEmail, nameof(command.RecipientEmail)).Deconstruct(out var recipientEmail),
                    EmailSubject.Create(command.Subject, nameof(command.Subject)).Deconstruct(out var subject))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendDocumentRequestNotice(initiatedBy!))
                .Ensure(() => State.DocumentRequests.ContainsKey(requestId!),
                    () => RequestErrors.NewNotFound("The document request was not found."))
                .Ensure(() => State.DocumentRequests[requestId!].RequestedFromUserId == (string)recipientUserId!,
                    () => RequestErrors.NewInvalidArg("The notice must go to the client the document was requested from."))
                .BindWithStreamTransaction(this, transaction =>
                {
                    // The notice is sent once per request; another attempt is an earlier attempt of the automation.
                    if (State.DocumentRequests[requestId!].NoticeSent)
                    {
                        return;
                    }
                    transaction.Apply(new DocumentRequestNoticeSent
                    {
                        OrderId = orderId!,
                        RequestId = requestId!,
                        RecipientUserId = recipientUserId!,
                        RecipientEmail = recipientEmail!,
                        Subject = subject!,
                        InitiatedBy = initiatedBy!,
                        OccurredAt = DateTimeOffset.UtcNow
                    });
                })
                .Map(response => new SendDocumentRequestNoticeResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }

    public async Task<Result<bool>> IsDocumentRequestNoticeSentAsync(string requestId)
    {
        return await Result.Success().BindTry(() =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return Task.FromResult(Result.CombineForValidation(
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId).Deconstruct(out var userId),
                    DocumentRequestId.Create(requestId, tenantId, nameof(requestId)).Deconstruct(out var validRequestId))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendDocumentRequestNotice(userId!))
                .Ensure(() => State.DocumentRequests.ContainsKey(validRequestId!),
                    () => RequestErrors.NewNotFound("The document request was not found."))
                .Map(() => State.DocumentRequests[validRequestId!].NoticeSent));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(DocumentRequestNoticeSent evt)
        => DocumentRequests[evt.RequestId] = DocumentRequests[evt.RequestId] with { NoticeSent = true };
}
