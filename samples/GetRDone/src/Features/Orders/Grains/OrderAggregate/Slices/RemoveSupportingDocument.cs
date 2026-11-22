using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<RemoveSupportingDocumentResponse>> RemoveSupportingDocumentAsync(RemoveSupportingDocumentCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(SupportingDocumentRemoved.InitiatedBy)).Deconstruct(out var initiatedBy),
                    DocumentId.Create(command.DocumentId, tenantId).Deconstruct(out var documentId))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order draft was not found."))
                .Bind(() => _permissions.CanChangeOrder(initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.RemoveSupportingDocument),
                    () => RequestErrors.NewFailedPrecondition("The order has been submitted and can no longer be changed."))
                .Ensure(() => State.SupportingDocumentBlobPaths.ContainsKey(documentId!),
                    () => RequestErrors.NewNotFound("The document is not attached to the order."))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new SupportingDocumentRemoved
                {
                    OrderId = orderId!,
                    DocumentId = documentId!,
                    BlobPath = State.SupportingDocumentBlobPaths[documentId!],
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new RemoveSupportingDocumentResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(SupportingDocumentRemoved evt) => SupportingDocumentBlobPaths.Remove(evt.DocumentId);
}
