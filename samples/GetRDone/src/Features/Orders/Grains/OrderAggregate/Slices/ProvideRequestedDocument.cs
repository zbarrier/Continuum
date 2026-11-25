using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<ProvideRequestedDocumentResponse>> ProvideRequestedDocumentAsync(ProvideRequestedDocumentCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(RequestedDocumentProvided.InitiatedBy)).Deconstruct(out var initiatedBy),
                    DocumentRequestId.Create(command.RequestId, tenantId, nameof(command.RequestId)).Deconstruct(out var requestId),
                    DocumentId.Create(command.DocumentId, tenantId).Deconstruct(out var documentId),
                    FileName.Create(command.FileName).Deconstruct(out var fileName),
                    ContentType.Create(command.ContentType).Deconstruct(out var contentType),
                    FileSize.Create(command.FileSize).Deconstruct(out var fileSize))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Ensure(() => State.DocumentRequests.ContainsKey(requestId!),
                    () => RequestErrors.NewNotFound("The document request was not found."))
                .Bind(() => _permissions.CanProvideRequestedDocument(initiatedBy!, State.DocumentRequests[requestId!]))
                .Ensure(() => !State.DocumentRequests[requestId!].Provided,
                    () => RequestErrors.NewFailedPrecondition("The requested document was already provided."))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.ProvideRequestedDocument),
                    () => RequestErrors.NewFailedPrecondition("The order no longer accepts requested documents."))
                .Ensure(() => !State.SupportingDocumentBlobPaths.ContainsKey(documentId!),
                    () => RequestErrors.NewAlreadyExists("The document is already attached to the order."))
                .BindWithStreamTransaction(this, transaction =>
                {
                    var request = State.DocumentRequests[requestId!];
                    transaction.Apply(new RequestedDocumentProvided
                    {
                        OrderId = orderId!,
                        RequestId = requestId!,
                        DocumentId = documentId!,
                        DocumentTypeId = request.DocumentTypeId,
                        DocumentTypeVersion = request.DocumentTypeVersion,
                        DocumentTypeName = request.DocumentTypeName,
                        FileName = fileName!,
                        ContentType = contentType!,
                        FileSize = fileSize!,
                        BlobPath = SupportingDocumentStorage.BlobPath(orderId!, documentId!),
                        InitiatedBy = initiatedBy!,
                        OccurredAt = DateTimeOffset.UtcNow
                    });
                })
                .Map(response => new ProvideRequestedDocumentResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(RequestedDocumentProvided evt)
    {
        SupportingDocumentBlobPaths[evt.DocumentId] = evt.BlobPath;
        DocumentRequests[evt.RequestId] = DocumentRequests[evt.RequestId] with { Provided = true };
    }
}
