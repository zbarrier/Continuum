using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<AttachSupportingDocumentResponse>> AttachSupportingDocumentAsync(AttachSupportingDocumentCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(SupportingDocumentAttached.InitiatedBy)).Deconstruct(out var initiatedBy),
                    DocumentId.Create(command.DocumentId, tenantId).Deconstruct(out var documentId),
                    DocumentTypeRef.Create(command.DocumentTypeId, command.DocumentTypeVersion, command.DocumentTypeName, tenantId)
                        .Deconstruct(out var documentType),
                    FileName.Create(command.FileName).Deconstruct(out var fileName),
                    ContentType.Create(command.ContentType).Deconstruct(out var contentType),
                    FileSize.Create(command.FileSize).Deconstruct(out var fileSize),
                    DocumentDescription.CreateOptional(command.Description, nameof(AttachSupportingDocumentCommand.Description))
                        .Deconstruct(out var description))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order draft was not found."))
                .Bind(() => _permissions.CanChangeOrder(initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.AttachSupportingDocument),
                    () => RequestErrors.NewFailedPrecondition("The order has been submitted and can no longer be changed."))
                .Ensure(() => !State.SupportingDocumentBlobPaths.ContainsKey(documentId!),
                    () => RequestErrors.NewAlreadyExists("The document is already attached to the order."))
                .Bind(() => GrainFactory.GetDocumentTypesGrain(tenantId).IsCurrentDocumentTypeAsync(documentType!))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new SupportingDocumentAttached
                {
                    OrderId = orderId!,
                    DocumentId = documentId!,
                    DocumentTypeId = documentType!.Id,
                    DocumentTypeVersion = documentType.Version,
                    DocumentTypeName = documentType.Name,
                    FileName = fileName!,
                    ContentType = contentType!,
                    FileSize = fileSize!,
                    Description = description?.ToString(),
                    BlobPath = SupportingDocumentStorage.BlobPath(orderId!, documentId!),
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new AttachSupportingDocumentResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(SupportingDocumentAttached evt) => SupportingDocumentBlobPaths[evt.DocumentId] = evt.BlobPath;
}
