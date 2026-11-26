using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<SendDescriptionOfWorkApprovedNoticeResponse>> SendDescriptionOfWorkApprovedNoticeAsync(
        SendDescriptionOfWorkApprovedNoticeCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(DescriptionOfWorkApprovedNoticeSent.InitiatedBy))
                        .Deconstruct(out var initiatedBy),
                    DescriptionOfWorkId.Create(command.DescriptionOfWorkId, tenantId, nameof(command.DescriptionOfWorkId))
                        .Deconstruct(out var descriptionOfWorkId),
                    UserId.Create(command.RecipientUserId, tenantId, nameof(command.RecipientUserId)).Deconstruct(out var recipientUserId),
                    EmailAddress.Create(command.RecipientEmail, nameof(command.RecipientEmail)).Deconstruct(out var recipientEmail),
                    EmailSubject.Create(command.Subject, nameof(command.Subject)).Deconstruct(out var subject))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendDescriptionOfWorkApprovedNotice(initiatedBy!))
                .Ensure(() => State.DescriptionOfWork?.Id == (string)descriptionOfWorkId!,
                    () => RequestErrors.NewNotFound("The description of work was not found."))
                .Ensure(() => State.DescriptionOfWork!.Approved,
                    () => RequestErrors.NewFailedPrecondition("The description of work is not approved yet."))
                .Ensure(() => State.AssignedTo == (string)recipientUserId!,
                    () => RequestErrors.NewInvalidArg("The notice must go to the staff member the order is assigned to."))
                .BindWithStreamTransaction(this, transaction =>
                {
                    // The notice is sent once; another attempt is an earlier attempt of the automation.
                    if (State.DescriptionOfWork!.ApprovedNoticeSent)
                    {
                        return;
                    }
                    transaction.Apply(new DescriptionOfWorkApprovedNoticeSent
                    {
                        OrderId = orderId!,
                        DescriptionOfWorkId = descriptionOfWorkId!,
                        RecipientUserId = recipientUserId!,
                        RecipientEmail = recipientEmail!,
                        Subject = subject!,
                        InitiatedBy = initiatedBy!,
                        OccurredAt = DateTimeOffset.UtcNow
                    });
                })
                .Map(response => new SendDescriptionOfWorkApprovedNoticeResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }

    public async Task<Result<bool>> IsDescriptionOfWorkApprovedNoticeSentAsync(string descriptionOfWorkId)
    {
        return await Result.Success().BindTry(() =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return Task.FromResult(Result.CombineForValidation(
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId).Deconstruct(out var userId),
                    DescriptionOfWorkId.Create(descriptionOfWorkId, tenantId, nameof(descriptionOfWorkId)).Deconstruct(out var validId))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendDescriptionOfWorkApprovedNotice(userId!))
                .Ensure(() => State.DescriptionOfWork?.Id == (string)validId!,
                    () => RequestErrors.NewNotFound("The description of work was not found."))
                .Map(() => State.DescriptionOfWork!.ApprovedNoticeSent));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(DescriptionOfWorkApprovedNoticeSent evt)
        => DescriptionOfWork = DescriptionOfWork! with { ApprovedNoticeSent = true };
}
