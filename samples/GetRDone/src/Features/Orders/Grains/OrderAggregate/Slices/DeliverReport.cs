using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<DeliverReportResponse>> DeliverReportAsync(DeliverReportCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(ReportDelivered.InitiatedBy)).Deconstruct(out var initiatedBy),
                    DocumentId.Create(command.DocumentId, tenantId).Deconstruct(out var documentId),
                    FileName.Create(command.FileName).Deconstruct(out var fileName),
                    ContentType.Create(command.ContentType).Deconstruct(out var contentType),
                    FileSize.Create(command.FileSize).Deconstruct(out var fileSize))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Ensure(() => State.VendorAssignment is not null,
                    () => RequestErrors.NewFailedPrecondition("The order is not assigned to a vendor yet."))
                .Bind(() => _permissions.CanDoVendorWorkAsync(initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.DeliverReport),
                    () => State.Report is not null
                        ? RequestErrors.NewFailedPrecondition("The report was already delivered.")
                        : RequestErrors.NewFailedPrecondition("The inspection must be completed before the report is delivered."))
                .Ensure(() => !State.SupportingDocumentBlobPaths.ContainsKey(documentId!),
                    () => RequestErrors.NewAlreadyExists("The document is already attached to the order."))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new ReportDelivered
                {
                    OrderId = orderId!,
                    DocumentId = documentId!,
                    FileName = fileName!,
                    ContentType = contentType!,
                    FileSize = fileSize!,
                    BlobPath = SupportingDocumentStorage.BlobPath(orderId!, documentId!),
                    AssignedToUserId = State.AssignedTo!,
                    OrderNumber = State.OrderNumber,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new DeliverReportResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(ReportDelivered evt)
    {
        Status = OrderStatus.ReportDelivered;
        SupportingDocumentBlobPaths[evt.DocumentId] = evt.BlobPath;
        Report = new OrderReport(evt.DocumentId, NoticeSent: false);
    }
}
