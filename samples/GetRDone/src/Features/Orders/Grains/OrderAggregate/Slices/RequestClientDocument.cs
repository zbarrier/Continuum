using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

using ManagedCode.Orleans.Identity.Core.Extensions;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<RequestClientDocumentResponse>> RequestClientDocumentAsync(RequestClientDocumentCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(ClientDocumentRequested.InitiatedBy)).Deconstruct(out var initiatedBy),
                    DocumentRequestId.Create(command.RequestId, tenantId, nameof(command.RequestId)).Deconstruct(out var requestId),
                    DocumentTypeRef.Create(command.DocumentTypeId, command.DocumentTypeVersion, command.DocumentTypeName, tenantId)
                        .Deconstruct(out var documentType),
                    DocumentRequestMessage.Create(command.Message, nameof(command.Message)).Deconstruct(out var message))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanRequestClientDocument(this.GetCurrentUser(), initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.RequestClientDocument),
                    () => RequestErrors.NewFailedPrecondition("The order must be acknowledged before a document can be requested."))
                .Ensure(() => !State.DocumentRequests.ContainsKey(requestId!),
                    () => RequestErrors.NewAlreadyExists("The document request already exists."))
                .Bind(() => GrainFactory.GetDocumentTypesGrain(tenantId).IsCurrentDocumentTypeAsync(documentType!))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new ClientDocumentRequested
                {
                    OrderId = orderId!,
                    RequestId = requestId!,
                    RequestedFromUserId = State.StartedBy!,
                    DocumentTypeId = documentType!.Id,
                    DocumentTypeVersion = documentType.Version,
                    DocumentTypeName = documentType.Name,
                    Message = message!,
                    OrderNumber = State.OrderNumber,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new RequestClientDocumentResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(ClientDocumentRequested evt)
        => DocumentRequests[evt.RequestId] = new OrderDocumentRequest(
            evt.RequestedFromUserId, evt.DocumentTypeId, evt.DocumentTypeVersion, evt.DocumentTypeName, NoticeSent: false, Provided: false);
}
