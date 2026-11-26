using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<SendVendorAssignedNoticeResponse>> SendVendorAssignedNoticeAsync(SendVendorAssignedNoticeCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
            var recipients = command.Recipients ?? [];
            var recipientsPath = PropertyPath.Root.Append(nameof(command.Recipients));

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(VendorAssignedNoticeSent.InitiatedBy))
                        .Deconstruct(out var initiatedBy),
                    VendorId.Create(command.VendorId, tenantId, nameof(command.VendorId)).Deconstruct(out var vendorId),
                    Result.NotEmpty(recipients, nameof(command.Recipients)),
                    Result.CombineForValidation(recipients.SelectMany((recipient, i) => new Result[]
                    {
                        UserId.Create(recipient?.UserId, tenantId, recipientsPath.Index(i), nameof(VendorNoticeRecipient.UserId)),
                        EmailAddress.Create(recipient?.Email, recipientsPath.Index(i), nameof(VendorNoticeRecipient.Email))
                    })),
                    EmailSubject.Create(command.Subject, nameof(command.Subject)).Deconstruct(out var subject))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendVendorAssignedNotice(initiatedBy!))
                .Ensure(() => State.VendorAssignment?.VendorId == (string)vendorId!,
                    () => RequestErrors.NewNotFound("The order is not assigned to the vendor."))
                .Bind(() => _permissions.AreVendorUsersAsync(vendorId!, recipients.Select(recipient => recipient.UserId).ToArray()))
                .BindWithStreamTransaction(this, transaction =>
                {
                    // The notice is sent once; another attempt is an earlier attempt of the automation.
                    if (State.VendorAssignment!.NoticeSent)
                    {
                        return;
                    }
                    transaction.Apply(new VendorAssignedNoticeSent
                    {
                        OrderId = orderId!,
                        VendorId = vendorId!,
                        Recipients = recipients,
                        Subject = subject!,
                        InitiatedBy = initiatedBy!,
                        OccurredAt = DateTimeOffset.UtcNow
                    });
                })
                .Map(response => new SendVendorAssignedNoticeResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }

    public async Task<Result<bool>> IsVendorAssignedNoticeSentAsync(string vendorId)
    {
        return await Result.Success().BindTry(() =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return Task.FromResult(Result.CombineForValidation(
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId).Deconstruct(out var userId),
                    VendorId.Create(vendorId, tenantId, nameof(vendorId)).Deconstruct(out var validId))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanSendVendorAssignedNotice(userId!))
                .Ensure(() => State.VendorAssignment?.VendorId == (string)validId!,
                    () => RequestErrors.NewNotFound("The order is not assigned to the vendor."))
                .Map(() => State.VendorAssignment!.NoticeSent));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(VendorAssignedNoticeSent evt)
        => VendorAssignment = VendorAssignment! with { NoticeSent = true };
}
